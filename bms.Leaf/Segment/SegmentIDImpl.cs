using bms.Leaf.Common;
using bms.Leaf.Segment.DAL.MySql;
using bms.Leaf.Segment.Model;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace bms.Leaf.Segment
{
    public class SegmentIDImpl : IDGen
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
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);
        public SegmentIDImpl(ILogger<SegmentIDImpl> logger, IAllocDAL allocDAL)
        {
            _logger = logger;
            _allocDAL = allocDAL;
        }

        public async Task<Result> GetAsync(string key)
        {
            if (!initOK)
            {
                return new Result(ExceptionIdIdcacheInitFalse, Status.EXCEPTION);
            }
            if (cache.TryGetValue(key, out SegmentBufferModel buffer))
            {
                if (!buffer.IsInitOk)
                {
                    await semaphore.WaitAsync();
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
                return await GetIdFromSegmentBufferAsync(buffer);
            }
            return new Result(ExceptionIdKeyNotExists, Status.EXCEPTION);
        }

        private async Task<Result> GetIdFromSegmentBufferAsync(SegmentBufferModel buffer)
        {
            while (true)
            {
                buffer.ReadWriteLock.EnterReadLock();
                try
                {
                    var segment = buffer.Current;
                    if (!buffer.IsNextReady && (segment.GetIdle() < 0.9 * segment.Step)
                        && buffer.ThreadRunning.CompareAndSet(false, true))
                    {
                        await Task.Run(async () =>
                        {
                            var next = buffer.Segments[buffer.NextPos()];
                            var updateOk = false;
                            try
                            {
                                await UpdateSegmentFromDbAsync(buffer.Key, next);
                                updateOk = true;
                                _logger.LogInformation($"update segment {buffer} from db {next}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, $"{buffer.Key} UpdateSegmentFromDbAsync exception");
                            }
                            finally
                            {
                                if (updateOk)
                                {
                                    buffer.ReadWriteLock.EnterWriteLock();
                                    buffer.IsNextReady = true;
                                    buffer.ThreadRunning.Set(false);
                                    buffer.ReadWriteLock.ExitWriteLock();
                                }
                                else
                                {
                                    buffer.ThreadRunning.Set(false);
                                }
                            }
                        });
                    }

                    long value = segment.Value.GetAndIncrement();
                    if (value < segment.Max)
                    {
                        return new Result(value, Status.SUCCESS);
                    }
                }
                finally
                {
                    buffer.ReadWriteLock.ExitReadLock();
                }

                await WaitAndSleepAsync(buffer);

                buffer.ReadWriteLock.EnterWriteLock();
                try
                {
                    SegmentModel segment = buffer.Current;
                    long value = segment.Value.GetAndIncrement();
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
                    buffer.ReadWriteLock.ExitWriteLock();
                }
            }
        }

        private async Task WaitAndSleepAsync(SegmentBufferModel buffer)
        {
            int roll = 0;
            while (buffer.ThreadRunning.Get())
            {
                roll += 1;
                if (roll > 10000)
                {
                    await Task.Delay(10);
                    break;
                }
            }
        }

        private async Task UpdateSegmentFromDbAsync(string key, SegmentModel segment)
        {
            var sw = new Stopwatch();
            sw.Start();
            var buffer = segment.Buffer;
            LeafAllocModel leafAlloc;
            if (!buffer.IsInitOk)
            {
                leafAlloc = await _allocDAL.UpdateMaxIdAndGetLeafAllocAsync(key);
                if (leafAlloc == null)
                {
                    _logger.LogWarning($"LeafAllocModel为NULL,{key}");
                    return;
                }
                buffer.Step = leafAlloc.Step;
                buffer.MinStep = leafAlloc.Step;
            }
            else if (buffer.UpdateTimestamp == 0)
            {
                leafAlloc = await _allocDAL.UpdateMaxIdAndGetLeafAllocAsync(key);
                if (leafAlloc == null)
                {
                    _logger.LogWarning($"LeafAllocModel为NULL,{key}");
                    return;
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

                var temp = new LeafAllocModel();
                temp.Key = key;
                temp.Step = nextStep;
                leafAlloc = await _allocDAL.UpdateMaxIdByCustomStepAndGetLeafAllocAsync(temp);
                if (leafAlloc == null)
                {
                    _logger.LogWarning($"LeafAllocModel为NULL,{key}");
                    return;
                }
                buffer.UpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                buffer.Step = leafAlloc.Step;
                buffer.MinStep = leafAlloc.Step;
            }
            var value = leafAlloc.MaxId - buffer.Step;
            segment.Value.Set(value);
            segment.Max = leafAlloc.MaxId;
            segment.Step = buffer.Step;
            sw.Stop();
            _logger.LogInformation($"UpdateSegmentFromDbAsync, {key} {JsonSerializer.Serialize(segment)}");
        }

        public async Task<bool> InitAsync()
        {
            _logger.LogInformation("Init ...");
            await UpdateCacheFromDbAsync();
            initOK = true;
            await UpdateCacheFromDbAtEveryMinuteAsync();
            return initOK;
        }

        private async Task UpdateCacheFromDbAtEveryMinuteAsync()
        {
            timer = new System.Timers.Timer(60000);
            timer.Elapsed += async (sender, e) => await HandleTimerAsync();
            timer.Start();
            await Task.CompletedTask;
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

        private async Task UpdateCacheFromDbAsync()
        {
            _logger.LogInformation("update cache from db");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                var dbTags = await _allocDAL.GetAllTagsAsync();
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
