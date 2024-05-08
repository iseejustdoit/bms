using bms.Leaf;
using bms.Leaf.Common;
using bms.Leaf.Segment;
using bms.Leaf.Segment.DAL.MySql;
using bms.Leaf.Segment.DAL.MySql.Impl;
using bms.Leaf.Snowflake;
using bms.Leaf.SnowFlake;
using bms.WebApi.Protos;
using FreeRedis;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

            //var loggerFactory = LoggerFactory.Create(builder =>
            //{
            //    builder.AddConsole();
            //});
            //var holderLogger = new Logger<SnowflakeRedisHolder>(loggerFactory);
            //var ip = Utils.GetIp();
            //var redisClient = new RedisClient("192.168.10.60:6379,defaultDatabase=0,password=123456");
            //ISnowflakeRedisHolder holder = new SnowflakeRedisHolder(holderLogger, redisClient, ip, "8080");
            //var logger = new Logger<SnowflakeIDGenImpl>(loggerFactory);
            //var idgen = new SnowflakeIDGenImpl(logger, holder);

            //await idgen.InitAsync();
            //Console.WriteLine("------------------- init completed");
            //var dict = new ConcurrentDictionary<long, long>();
            //var sumDict = new ConcurrentDictionary<int, long>();
            //for (int i = 0; i < 30; i++)
            //{
            //    int count = 0;
            //    var stopWatch = Stopwatch.StartNew();

            //    // 使用所有CPU核心并行执行
            //    Parallel.Invoke(new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, () =>
            //    {
            //        while (stopWatch.ElapsedMilliseconds < 1000)
            //        {
            //            var result = idgen.GetAsync("leaf-segment-test").GetAwaiter().GetResult();

            //            if (!dict.TryAdd(result.Id, result.Id))
            //            {
            //                Console.WriteLine("......... 失败");
            //            }
            //            Interlocked.Increment(ref count);
            //        }
            //    });

            //    sumDict.TryAdd(i, count);
            //    Console.WriteLine($"第{i + 1}次统计，一秒钟内方法被调用了{count}次。");
            //}
            //Console.WriteLine($"GetId 平均一秒内调用方法次数：统计次数： {sumDict.Count}  {(double)sumDict.Values.Sum() / sumDict.Count}");
            var url = "http://localhost:5001";
            using (var channel = GrpcChannel.ForAddress(url))
            {
                 var client = new IdGenerator.IdGeneratorClient(channel);
                // 调用GetSegmentId方法
                var segmentIdResponse = await client.GetSegmentIdAsync(new KeyRequest { Key = "leaf-segment-test" });
                Console.WriteLine($"SegmentId: {segmentIdResponse.Id}");

                // 调用GetSnowflakeId方法
                var snowflakeIdResponse = await client.GetSnowflakeIdAsync(new KeyRequest { Key = "leaf-segment-test" });
                Console.WriteLine($"SnowflakeId: {snowflakeIdResponse.Id}");
            }

            Console.ReadLine();
        }
    }
}
