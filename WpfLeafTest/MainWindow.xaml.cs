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
        private SnowflakeIDGenImpl? idgen;
        private readonly ITextService textService;

        public MainWindow(ITextService textService)
        {
            InitializeComponent();

            this.textService = textService;
            Init().ConfigureAwait(false);
        }

        private async Task Init()
        {
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
            await idgen.InitAsync();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (idgen == null)
            {
                MessageBox.Show("ID generator is not initialized.");
                return;
            }
            textService.Plus();
            var service = App.Current.ServiceProvider.GetRequiredService<ITextService>();
            var testWin = new Test(service);

            testWin.Show();
            var result = await idgen.GetAsync("leaf-segment-test");
            MessageBox.Show($"{result.Id},{result.Status}");
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (idgen == null)
            {
                MessageBox.Show("ID generator is not initialized.");
                return;
            }
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
            if (idgen == null)
            {
                MessageBox.Show("ID generator is not initialized.");
                return;
            }
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
            if (idgen == null)
            {
                MessageBox.Show("ID generator is not initialized.");
                return;
            }
            var result = await idgen.GetAsync("leaf-segment-test");
            MessageBox.Show($"{result.Id},{result.Status}");
        }
    }
}