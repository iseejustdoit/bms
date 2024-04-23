using bms.Leaf.Snowflake.Exceptions;
using bms.Leaf.SnowFlake;
using Bms.Leaf.Snowflake;
using FreeRedis;
using Microsoft.Extensions.Logging;

namespace bms.Leaf.Snowflake
{
    public class SnowflakeRedisHolder : ISnowflakeRedisHolder
    {
        private readonly ILogger _logger;
        private string listenAddress = null;
        private int workerId;
        private long schedulePeriod = 5;
        private static readonly string PersistentTimeName = Constant.PersistentName + "-time";
        private static readonly string PersistentTimeKey = Constant.RootName + Constant.Colon + PersistentTimeName;
        private static readonly string PersistentKey = Constant.RootName + Constant.Colon + Constant.PersistentName;
        private static readonly string EphemeralKey = Constant.RootName + Constant.Colon + Constant.EphemeralName;
        private string connectionString;
        private long lastUpdateTime;
        private readonly string ip;
        private readonly string port;
        private RedisClient cli;
        private Timer timer;

        public SnowflakeRedisHolder(ILogger<SnowflakeRedisHolder> logger, string ip, string port, string connectionString)
        {
            _logger = logger;
            this.ip = ip;
            this.port = port;
            listenAddress = ip + ":" + port;
            this.connectionString = connectionString;

            timer = new Timer(ScheduledUploadData, null, Timeout.Infinite, Timeout.Infinite);
        }

        public bool Init()
        {
            try
            {
                cli = new RedisClient(connectionString);
                var time = cli.HGet(PersistentTimeKey, listenAddress);

                if (time != null && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < long.Parse(time))
                {
                    throw new CheckLastTimeException("init timestamp check error,redis node timestamp gt this node time");
                }

                var sequentialObj = cli.Eval(Constant.RedisAddPersistentScript,
                    new string[] { PersistentKey, PersistentTimeKey, listenAddress }, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                workerId = Convert.ToInt32(sequentialObj);

                // Start the timer
                timer.Change(schedulePeriod, Timeout.Infinite);

                UpdateLocalWorkerId(workerId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Start node ERROR");
                var workerIdResult = ReadLocalWorkerId();
                if (workerIdResult == null) return false;
                this.workerId = workerIdResult.Value;
            }

            return true;
        }

        public int GetWorkerId()
        {
            return workerId;
        }

        private int? ReadLocalWorkerId()
        {
            var fileInfo = GetLocalFilePath();
            int? workerId = null;

            try
            {
                if (fileInfo.Exists)
                {
                    using var sr = new StreamReader(fileInfo.FullName);
                    string content = sr.ReadToEnd();
                    workerId = int.Parse(content);

                    _logger.LogInformation($"Read workerID from local file: {workerId}");
                }
                else
                {
                    _logger.LogWarning($"File does not exist: {fileInfo.FullName}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error reading workerID from local file");
            }

            return workerId;
        }

        private void UpdateLocalWorkerId(int workerId)
        {
            var fileInfo = GetLocalFilePath();

            try
            {
                string parentDirectory = fileInfo.DirectoryName;
                if (!Directory.Exists(parentDirectory))
                {
                    Directory.CreateDirectory(parentDirectory);
                }

                using (var sw = new StreamWriter(fileInfo.FullName, false))
                {
                    sw.Write(workerId.ToString());
                }

                _logger.LogInformation($"local file cache workerID is {workerId}");
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "create workerID conf file error");
            }
        }

        private FileInfo GetLocalFilePath()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "leafconf", Constant.RootName, Constant.Strike, ip, Constant.Strike, port);

            _logger.LogDebug($"获取本地缓存文件路径{filePath}");

            return new FileInfo(filePath);
        }

        private async Task ScheduledUploadDataAsync()
        {
            try
            {
                long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (currentTimestamp < lastUpdateTime)
                {
                    return;
                }
                var ephemeralKey = EphemeralKey + Constant.Colon + listenAddress;
                long expire = schedulePeriod + 10;
                await cli.EvalAsync(Constant.RedisUpdateEphemeralScript, new string[] { ephemeralKey, PersistentTimeKey, listenAddress },
                        currentTimestamp, expire);
                lastUpdateTime = currentTimestamp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "update init data error");
            }
            finally
            {
                // 任务完成后，重新设置定时器
                timer.Change(schedulePeriod, Timeout.Infinite);
            }
        }

        private void ScheduledUploadData(object state)
        {
            _ = ScheduledUploadDataAsync();
        }

        public void Dispose()
        {
            timer?.Dispose();
            cli?.Dispose();
        }
    }
}
