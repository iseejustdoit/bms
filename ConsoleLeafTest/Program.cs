using bms.Leaf.Segment;
using bms.Leaf.Segment.DAL.MySql;
using bms.Leaf.Segment.DAL.MySql.Impl;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ConsoleLeafTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var connectionString = "DataBase=leaf;Data Source=192.168.10.60;Port=3306;User Id=root;Password=123456;";
            IAllocDAL dal = new AllocDALImpl(connectionString);
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Error);
            });
            var logger = new Logger<SegmentIDGenImpl>(loggerFactory);
            var idgen = new SegmentIDGenImpl(logger, dal);

            await idgen.InitAsync();

            var dict = new ConcurrentDictionary<long, long>();

            int testRuns = 50;

            for (int i = 0; i < testRuns; i++)
            {
                var stopwatch = new Stopwatch();
                int counter = 0;

                stopwatch.Start();

                // 创建一个任务列表
                var tasks = new List<Task>();

                for (int j = 0; j < Environment.ProcessorCount; j++)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        while (stopwatch.ElapsedMilliseconds < 1000)
                        {
                            var result = await idgen.GetAsync("leaf-segment-test");

                            dict.TryAdd(result.Id, result.Id);
                            Interlocked.Increment(ref counter);
                        }
                    }));
                }

                // 等待所有任务完成
                await Task.WhenAll(tasks);

                stopwatch.Stop();

                Console.WriteLine($"第 {i + 1} 次运行，一秒钟内，idgen.GetAsync() 方法被调用了 {counter} 次。");
            }
            Console.ReadLine();

        }
    }
}
