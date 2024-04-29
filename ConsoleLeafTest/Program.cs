using bms.Leaf;
using bms.Leaf.Segment;
using bms.Leaf.Segment.DAL.MySql;
using bms.Leaf.Segment.DAL.MySql.Impl;
using bms.Leaf.Snowflake;
using bms.Leaf.SnowFlake;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ConsoleLeafTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //var connectionString = "DataBase=leaf;Data Source=192.168.10.60;Port=3306;User Id=root;Password=123456;";
            //IAllocDAL dal = new AllocDALImpl(connectionString);
            //var loggerFactory = LoggerFactory.Create(builder =>
            //{
            //    builder.AddConsole();
            //    builder.SetMinimumLevel(LogLevel.Error);
            //});
            //var logger = new Logger<SegmentIDGenImpl>(loggerFactory);
            //var idgen = new SegmentIDGenImpl(logger, dal);

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            var holderLogger = new Logger<SnowflakeRedisHolder>(loggerFactory);
            var ip = GetLocalIPAddressWithNetworkInterface(NetworkInterfaceType.Ethernet);
            ISnowflakeRedisHolder holder = new SnowflakeRedisHolder(holderLogger, ip, "8080", "192.168.10.60:6379,defaultDatabase=0,password=123456");
            var logger = new Logger<SnowflakeIDGenImpl>(loggerFactory);
            var idgen = new SnowflakeIDGenImpl(logger, holder);

            await idgen.InitAsync();
            Console.WriteLine("------------------- init completed");
            var dict = new ConcurrentDictionary<long, long>();
            for (int i = 0; i < 30; i++)
            {
                int count = 0;
                var stopWatch = Stopwatch.StartNew();

                // 使用所有CPU核心并行执行
                Parallel.Invoke(new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, () =>
                {
                    while (stopWatch.ElapsedMilliseconds < 1000)
                    {
                        var result = idgen.GetAsync("leaf-segment-test").GetAwaiter().GetResult();

                        if (!dict.TryAdd(result.Id, result.Id))
                        {
                            Console.WriteLine("......... 失败");
                        }
                        Interlocked.Increment(ref count);
                    }
                });

                Console.WriteLine($"第{i + 1}次统计，一秒钟内方法被调用了{count}次。");
            }

            Console.ReadLine();
        }
        public static string GetLocalIPAddressWithNetworkInterface(NetworkInterfaceType _type)
        {
            string output = "";
            var isBreak = false;
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                            isBreak = true;
                            break;
                        }
                    }
                }
                if (isBreak)
                    break;
            }
            return output;
        }
    }
}
