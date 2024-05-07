using bms.Leaf;
using bms.Leaf.Common;
using bms.Leaf.Snowflake;
using bms.Leaf.SnowFlake;
using FreeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace WpfLeafTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IIDGen idgen;
        private readonly ITextService textService;

        public MainWindow(ITextService textService)
        {
            InitializeComponent();

            Init();

            this.textService = textService;
        }

        private async void Init()
        {
            //var connectionString = "DataBase=leaf;Data Source=192.168.10.60;Port=3306;User Id=root;Password=123456;";
            //IAllocDAL dal = new AllocDALImpl(connectionString);
            //var loggerFactory = LoggerFactory.Create(builder =>
            //{
            //    builder.AddConsole();
            //});
            //var logger = new Logger<SegmentIDGenImpl>(loggerFactory);
            //idgen = new SegmentIDGenImpl(logger, dal);

            //await idgen.InitAsync();

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            var holderLogger = new Logger<SnowflakeRedisHolder>(loggerFactory);
            var ip = Utils.GetIp();
            var redisClient = new RedisClient("192.168.10.60:6379,defaultDatabase=0,password=123456");
            ISnowflakeRedisHolder holder = new SnowflakeRedisHolder(holderLogger, redisClient, ip, "8080");
            var logger = new Logger<SnowflakeIDGenImpl>(loggerFactory);
            idgen = new SnowflakeIDGenImpl(logger, holder);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            textService.Plus();
            var service = App.Current.ServiceProvider.GetRequiredService<ITextService>();
            var testWin = new Test(service);

            testWin.Show();
            var result = await idgen.GetAsync("leaf-segment-test");
            MessageBox.Show($"{result.Id},{result.Status}");
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var text = textService.GetText();
            var idList = new List<long>();
            for (int i = 0; i < 10; i++)
            {
                var idResult = await idgen.GetAsync("leaf-segment-test");
                if (idResult.Status == Status.SUCCESS)
                    idList.Add(idResult.Id);
            }
            MessageBox.Show(string.Join(",", idList));
        }

        private async void Id100(object sender, RoutedEventArgs e)
        {
            var idList = new List<long>();
            for (int i = 0; i < 100; i++)
            {
                var idResult = await idgen.GetAsync("leaf-segment-test");
                if (idResult.Status == Status.SUCCESS)
                    idList.Add(idResult.Id);
            }
            MessageBox.Show(string.Join(",", idList));
        }

        private async void SnowflakeId_Click(object sender, RoutedEventArgs e)
        {
            var result = await idgen.GetAsync("leaf-segment-test");
            MessageBox.Show($"{result.Id},{result.Status}");
        }
    }
}