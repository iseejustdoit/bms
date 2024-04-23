using bms.Leaf;
using bms.Leaf.Common;
using bms.Leaf.Segment;
using bms.Leaf.Segment.DAL.MySql;
using bms.Leaf.Segment.DAL.MySql.Impl;
using bms.Leaf.Snowflake;
using bms.Leaf.SnowFlake;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows;

namespace WpfLeafTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IDGen idgen;
        public MainWindow()
        {
            InitializeComponent();

            Init();
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
            var ip = GetLocalIPAddressWithNetworkInterface(NetworkInterfaceType.Ethernet);
            ISnowflakeRedisHolder holder = new SnowflakeRedisHolder(holderLogger, ip, "8080", "172.29.89.20:6379,defaultDatabase=0");
            var logger = new Logger<SnowflakeIDGenImpl>(loggerFactory);
            idgen = new SnowflakeIDGenImpl(logger, holder);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var result = await idgen.GetAsync("leaf-segment-test");
            MessageBox.Show($"{result.Id},{result.Status}");
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
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

        public string GetLocalIPAddressWithNetworkInterface(NetworkInterfaceType _type)
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