using bms.Leaf.Common;
using bms.Leaf.SnowFlake;
using Microsoft.Extensions.Logging;

namespace bms.Leaf.Snowflake
{
    public class SnowflakeIDGenImpl : IDGen
    {
        private readonly ILogger _logger;
        private readonly long twepoch;
        private readonly long workerIdBits = 10L;
        private readonly long maxWorkerId;
        private readonly long sequenceBits = 12L;
        private readonly long workerIdShift;
        private readonly long timestampLeftShift;
        private readonly long sequenceMask;
        private long workerId;
        private long sequence = 0L;
        private long lastTimestamp = -1L;
        private static readonly ThreadLocal<Random> Random = new ThreadLocal<Random>(() => new Random());

        public SnowflakeIDGenImpl(ILogger<SnowflakeIDGenImpl> logger, ISnowflakeRedisHolder snowflakeRedisHolder)
            : this(logger, snowflakeRedisHolder, 1288834974657L)
        {
        }

        public SnowflakeIDGenImpl(ILogger<SnowflakeIDGenImpl> logger, ISnowflakeRedisHolder snowflakeRedisHolder, long twepoch)
        {
            _logger = logger;
            this.twepoch = twepoch;
            this.workerIdBits = 10L;
            this.maxWorkerId = ~(-1L << (int)workerIdBits);
            this.sequenceBits = 12L;
            this.workerIdShift = sequenceBits;
            this.timestampLeftShift = sequenceBits + workerIdBits;
            this.sequenceMask = ~(-1L << (int)sequenceBits);

            if (TimeGen() <= twepoch)
                throw new ArgumentException("Snowflake not support twepoch gt currentTime");

            bool initFlag = snowflakeRedisHolder.Init();
            if (initFlag)
            {
                workerId = snowflakeRedisHolder.GetWorkerId();
                _logger.LogInformation($"START SUCCESS USE Redis WORKERID-{workerId}");
            }
            else
            {
                throw new ArgumentException("Snowflake Id Gen is not init ok");
            }
            if (workerId < 0 || workerId > maxWorkerId)
                throw new ArgumentException("workerID must gte 0 and lte 1023");
        }

        public long GetWorkerId()
        {
            return workerId;
        }

        public async Task<bool> InitAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(true);
        }

        public async Task<Result> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            long timestamp = TimeGen();
            if (timestamp < lastTimestamp)
            {
                long offset = lastTimestamp - timestamp;
                if (offset <= 5)
                {
                    try
                    {
                        await Task.Delay((int)(offset << 1), cancellationToken);
                        timestamp = TimeGen();
                        if (timestamp < lastTimestamp)
                        {
                            return new Result(-1, Status.EXCEPTION);
                        }
                    }
                    catch (OperationCanceledException e)
                    {
                        _logger.LogError(e, "wait interrupted");
                        return new Result(-2, Status.EXCEPTION);
                    }
                }
                else
                {
                    return new Result(-3, Status.EXCEPTION);
                }
            }
            if (lastTimestamp == timestamp)
            {
                sequence = (sequence + 1) & sequenceMask;
                if (sequence == 0)
                {
                    sequence = Random.Value.Next(100);
                    timestamp = TilNextMillis(lastTimestamp);
                }
            }
            else
            {
                sequence = Random.Value.Next(100);
            }
            lastTimestamp = timestamp;
            long id = ((timestamp - twepoch) << (int)timestampLeftShift) | (workerId << (int)workerIdShift) | sequence;
            return new Result(id, Status.SUCCESS);
        }

        protected long TilNextMillis(long lastTimestamp)
        {
            long timestamp = TimeGen();
            while (timestamp <= lastTimestamp)
            {
                timestamp = TimeGen();
            }
            return timestamp;
        }

        protected long TimeGen()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
