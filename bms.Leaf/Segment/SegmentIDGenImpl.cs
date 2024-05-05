using bms.Leaf.Common;
using bms.Leaf.Segment.DAL.MySql;
using bms.Leaf.Segment.Model;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace bms.Leaf.Segment
{
    public class SegmentIDGenImpl : IDGen
    {
        private readonly ILogger _logger;
        private readonly IAllocDAL _allocDAL;

        /// <summary>
        /// IDCache未初始化成功时的异常码
        /// </summary>
        private const long ExceptionIdIdcacheInitFalse = -1;
        /// <summary>
        ///  key不存在时的异常码
        /// </summary>
        private const long ExceptionIdKeyNotExists = -2;
        /// <summary>
        /// SegmentBuffer中的两个Segment均未从DB中装载时的异常码
        /// </summary>
        private const long ExceptionInTwoSegmentsAreNull = -3;
        /// <summary>
        /// 最大步长不超过100,0000
        /// </summary>
        private const int MaxStep = 1000000;
        /// <summary>
        /// 一个Segment维持时间为15分钟
        /// </summary>
        private const long SegmentDuration = 15 * 60 * 1000L;

        private volatile bool initOK = false;
        private ConcurrentDictionary<string, SegmentBufferModel> cache = new ConcurrentDictionary<string, SegmentBufferModel>();
        private System.Timers.Timer timer;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private static readonly object lockObj = new object();
        public SegmentIDGenImpl(ILogger<SegmentIDGenImpl> logger, IAllocDAL allocDAL)
        {
            _logger = logger;
            _allocDAL = allocDAL;
        }

        public async Task<Result> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            if (!initOK)
            {
                return new Result(ExceptionIdIdcacheInitFalse, Status.EXCEPTION);
            }
            if (cache.TryGetValue(key, out SegmentBufferModel buffer))
            {
                if (!buffer.IsInitOk)
                {
                    await semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        if (!buffer.IsInitOk)
                        {
                            try
                            {
                                await UpdateSegmentFromDbAsync(key, buffer.Current);
                                _logger.LogInformation($"Init buffer. Update leafkey {key} {buffer.Current} from db");
                                buffer.IsInitOk = true;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, $"Init buffer {buffer.Current} exception");
                            }
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }
                return await GetIdFromSegmentBufferAsync(buffer, cancellationToken);
            }
            return new Result(ExceptionIdKeyNotExists, Status.EXCEPTION);
        }

        private async Task<Result> GetIdFromSegmentBufferAsync(SegmentBufferModel buffer, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                buffer.Lock.EnterUpgradeableReadLock();
                try
                {
                    var segment = buffer.Current;
                    if (!buffer.IsNextReady && (segment.GetIdle() < 0.9 * segment.Step)
                        && buffer.ThreadRunning.CompareAndSet(false, true))
                    {
                        _ = UpdateNextSegmentFromDbAsync(buffer, cancellationToken);
                    }

                    long value = segment.Value.GetAndIncrement();
                    if (value < segment.Max)
                    {
                        return new Result(value, Status.SUCCESS);
                    }
                }
                finally
                {
                    buffer.Lock.ExitUpgradeableReadLock();
                }

                await WaitAndSleepAsync(buffer, cancellationToken);

                buffer.Lock.EnterWriteLock();
                try
                {
                    var segment = buffer.Current;
                    var value = segment.Value.GetAndIncrement();
                    if (value < segment.Max)
                    {
                        return new Result(value, Status.SUCCESS);
                    }
                    if (buffer.IsNextReady)
                    {
                        buffer.SwitchPos();
                        buffer.IsNextReady = false;
                    }
                    else
                    {
                        _logger.LogError($"Both two segments in {buffer} are not ready!");
                        return new Result(ExceptionInTwoSegmentsAreNull, Status.EXCEPTION);
                    }
                }
                finally
                {
                    buffer.Lock.ExitWriteLock();
                }
            }
        }

        private async Task UpdateNextSegmentFromDbAsync(SegmentBufferModel buffer, CancellationToken cancellationToken)
        {
            var next = buffer.Segments[buffer.NextPos()];
            var updateOk = false;
            try
            {
                await UpdateSegmentFromDbAsync(buffer.Key, next, cancellationToken);
                updateOk = true;
                _logger.LogInformation($"update segment {buffer} from db {next}");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"{buffer.Key} UpdateSegmentFromDbAsync was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"{buffer.Key} UpdateSegmentFromDbAsync exception");
            }
            finally
            {
                if (updateOk)
                {
                    buffer.Lock.EnterWriteLock();
                    try
                    {
                        buffer.IsNextReady = true;
                        buffer.ThreadRunning.Set(false);
                    }
                    finally
                    {
                        buffer.Lock.ExitWriteLock();
                    }
                }
                else
                {
                    buffer.ThreadRunning.Set(false);
                }
            }
        }

        private async Task WaitAndSleepAsync(SegmentBufferModel buffer, CancellationToken cancellationToken = default)
        {
            int roll = 0;
            while (buffer.ThreadRunning.Get())
            {
                roll += 1;
                if (roll > 10000)
                {
                    await Task.Delay(10, cancellationToken);
                    roll = 0;
                }
            }
        }

        private async Task UpdateSegmentFromDbAsync(string key, SegmentModel segment, CancellationToken cancellationToken = default)
        {
            var buffer = segment.Buffer;
            LeafAlloc leafAlloc;
            if (!buffer.IsInitOk)
            {
                leafAlloc = await _allocDAL.UpdateMaxIdAndGetLeafAllocAsync(key, cancellationToken);
                buffer.Step = leafAlloc.Step;
                buffer.MinStep = leafAlloc.Step;
            }
            else if (buffer.UpdateTimestamp == 0)
            {
                leafAlloc = await _allocDAL.UpdateMaxIdAndGetLeafAllocAsync(key, cancellationToken);
                buffer.UpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                buffer.Step = leafAlloc.Step;
                buffer.MinStep = leafAlloc.Step;
            }
            else
            {
                var duration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - buffer.UpdateTimestamp;
                var nextStep = buffer.Step;
                if (duration < SegmentDuration)
                {
                    if (nextStep * 2 > MaxStep)
                    {
                        // do nothing
                    }
                    else
                    {
                        nextStep = nextStep * 2;
                    }
                }
                else if (duration < SegmentDuration * 2)
                {
                    // do nothing with nextstep
                }
                else
                {
                    nextStep = nextStep / 2 >= buffer.MinStep ? nextStep / 2 : nextStep;
                }
                _logger.LogInformation($"leafKey[{key}], step[{buffer.Step}], duration[{(double)duration / (1000 * 60):0.00}mins],nextStep[{nextStep}]");

                var temp = new LeafAlloc();
                temp.BizTag = key;
                temp.Step = nextStep;
                leafAlloc = await _allocDAL.UpdateMaxIdByCustomStepAndGetLeafAllocAsync(temp, cancellationToken);
                buffer.UpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                buffer.Step = nextStep;
                buffer.MinStep = leafAlloc.Step;
            }
            var value = leafAlloc.MaxId - buffer.Step;
            segment.Value.Set(value);
            segment.Max = leafAlloc.MaxId;
            segment.Step = buffer.Step;

            _logger.LogInformation($"UpdateSegmentFromDbAsync, {key} {segment}");
        }

        public async Task<bool> InitAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Init ...");
            await UpdateCacheFromDbAsync(cancellationToken);
            initOK = true;
            UpdateCacheFromDbAtEveryMinute();
            return initOK;
        }

        private void UpdateCacheFromDbAtEveryMinute()
        {
            timer = new System.Timers.Timer(60000);
            timer.Elapsed += async (sender, e) => await HandleTimerAsync();
            timer.Start();
        }

        private async Task HandleTimerAsync()
        {
            timer.Stop();
            try
            {
                await UpdateCacheFromDbAsync();
            }
            finally
            {
                timer.Start();
            }
        }

        private async Task UpdateCacheFromDbAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("update cache from db");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                var dbTags = await _allocDAL.GetAllTagsAsync(cancellationToken);
                if (dbTags == null || !dbTags.Any())
                {
                    return;
                }
                List<string> cacheTags = new List<string>(cache.Keys);
                HashSet<string> insertTagsSet = new HashSet<string>(dbTags);
                HashSet<string> removeTagsSet = new HashSet<string>(cacheTags);
                // Add new tags from db to cache
                foreach (string tmp in cacheTags)
                {
                    if (insertTagsSet.Contains(tmp))
                    {
                        insertTagsSet.Remove(tmp);
                    }
                }
                foreach (string tag in insertTagsSet)
                {
                    SegmentBufferModel buffer = new SegmentBufferModel();
                    buffer.Key = tag;
                    SegmentModel segment = buffer.Current;
                    segment.Value = new AtomicLong(0);
                    segment.Max = 0;
                    segment.Step = 0;
                    cache[tag] = buffer;
                    _logger.LogInformation($"Add tag {tag} from db to IdCache, SegmentBuffer {buffer}");
                }
                // Remove invalid tags from cache
                foreach (string tmp in dbTags)
                {
                    if (removeTagsSet.Contains(tmp))
                    {
                        removeTagsSet.Remove(tmp);
                    }
                }
                foreach (string tag in removeTagsSet)
                {
                    cache.TryRemove(tag, out _);
                    _logger.LogInformation($"Remove tag {tag} from IdCache");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Update cache from db exception");
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"updateCacheFromDb elapsed time: {sw.ElapsedMilliseconds} ms");
            }
        }
    }
}
