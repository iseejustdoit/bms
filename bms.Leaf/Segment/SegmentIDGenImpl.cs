using bms.Leaf.Common;
using bms.Leaf.Entity;
using bms.Leaf.Segment.DAL.MySql;
using bms.Leaf.Segment.Model;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace bms.Leaf.Segment
{
    public class SegmentIDGenImpl : IIDGen
    {
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
        private readonly ConcurrentDictionary<string, SegmentBufferModel> cache = new();
        private readonly System.Timers.Timer timer;
        private readonly SemaphoreSlim semaphore = new(1, 1);
        private readonly ILogger<SegmentIDGenImpl> _logger;
        private readonly IAllocDAL _allocDAL;

        public string Name => "Segment";

        public SegmentIDGenImpl(ILogger<SegmentIDGenImpl> logger, IAllocDAL allocDAL)
        {
            this._logger = logger;
            this._allocDAL = allocDAL;

            // 初始化 timer 字段
            timer = new System.Timers.Timer(60000);
            timer.Elapsed += async (sender, e) => await HandleTimerAsync();
        }

        public async Task<Result> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            if (!initOK)
            {
                return new Result(ExceptionIdIdcacheInitFalse, Status.EXCEPTION);
            }
            if (cache.TryGetValue(key, out SegmentBufferModel? buffer) && buffer != null)
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
                                await UpdateSegmentFromDbAsync(key, buffer.Current, cancellationToken);
                                _logger.LogInformation("Init buffer. Update leafkey {key} {current} from db", key, buffer.Current);
                                buffer.IsInitOk = true;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Init buffer {current} exception", buffer.Current);
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
                        _logger.LogError("Both two segments in {buffer} are not ready!", buffer);
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
                await UpdateSegmentFromDbAsync(buffer.Key!, next, cancellationToken);
                updateOk = true;
                _logger.LogInformation("update segment {buffer} from db {next}", buffer, next);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("{key} UpdateSegmentFromDbAsync was cancelled", buffer.Key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{key} UpdateSegmentFromDbAsync exception", buffer.Key);
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

        private static async Task WaitAndSleepAsync(SegmentBufferModel buffer, CancellationToken cancellationToken = default)
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
            LeafAlloc? leafAlloc;
            if (!buffer.IsInitOk)
            {
                leafAlloc = await _allocDAL.UpdateMaxIdAndGetLeafAllocAsync(key, cancellationToken);
                if (leafAlloc == null)
                {
                    throw new InvalidOperationException("LeafAlloc cannot be null");
                }
                buffer.Step = leafAlloc.Step;
                buffer.MinStep = leafAlloc.Step;
            }
            else if (buffer.UpdateTimestamp == 0)
            {
                leafAlloc = await _allocDAL.UpdateMaxIdAndGetLeafAllocAsync(key, cancellationToken);
                if (leafAlloc == null)
                {
                    throw new InvalidOperationException("LeafAlloc cannot be null");
                }
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
                        nextStep *= 2;
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
                _logger.LogInformation("leafKey[{key}], step[{step}], duration[{duration}mins],nextStep[{nextStep}]", key, buffer.Step, ((double)duration / (1000 * 60)).ToString("F2"), nextStep);

                var temp = new LeafAlloc
                {
                    BizTag = key,
                    Step = nextStep
                };
                leafAlloc = await _allocDAL.UpdateMaxIdByCustomStepAndGetLeafAllocAsync(temp, cancellationToken);
                if (leafAlloc == null)
                {
                    throw new InvalidOperationException("LeafAlloc cannot be null");
                }
                buffer.UpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                buffer.Step = nextStep;
                buffer.MinStep = leafAlloc.Step;
            }
            var value = leafAlloc.MaxId - buffer.Step;
            segment.Value.Set(value);
            segment.Max = leafAlloc.MaxId;
            segment.Step = buffer.Step;

            _logger.LogInformation("UpdateSegmentFromDbAsync, {key} {segment}", key, segment);
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
            Stopwatch sw = new();
            sw.Start();
            try
            {
                var dbTags = await _allocDAL.GetAllTagsAsync(cancellationToken);
                if (dbTags == null || dbTags.Count == 0)
                {
                    return;
                }
                List<string> cacheTags = new(cache.Keys);
                HashSet<string> insertTagsSet = new(dbTags);
                HashSet<string> removeTagsSet = new(cacheTags);
                // Add new tags from db to cache
                foreach (string tmp in cacheTags)
                {
                    insertTagsSet.Remove(tmp);
                }
                foreach (string tag in insertTagsSet)
                {
                    SegmentBufferModel buffer = new()
                    {
                        Key = tag
                    };
                    SegmentModel segment = buffer.Current;
                    segment.Value = new AtomicLong(0);
                    segment.Max = 0;
                    segment.Step = 0;
                    cache[tag] = buffer;
                    _logger.LogInformation("Add tag {tag} from db to IdCache, SegmentBuffer {buffer}", tag, buffer);
                }
                // Remove invalid tags from cache
                foreach (string tmp in dbTags)
                {
                    removeTagsSet.Remove(tmp);
                }
                foreach (string tag in removeTagsSet)
                {
                    cache.TryRemove(tag, out _);
                    _logger.LogInformation("Remove tag {tag} from IdCache", tag);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Update cache from db exception");
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation("updateCacheFromDb elapsed time: {ms} ms", sw.ElapsedMilliseconds);
            }
        }
    }
}
