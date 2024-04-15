using bms.Leaf;
using bms.Leaf.Common;
using bms.Leaf.Segment;
using bms.Leaf.Segment.DAL.MySql;
using bms.Leaf.Segment.DAL.MySql.Impl;
using Microsoft.Extensions.Logging;
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
            var connectionString = "DataBase=leaf;Data Source=10.100.193.24;Port=3306;User Id=root;Password=123456;";
            IAllocDAL dal = new AllocDALImpl(connectionString);
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            var logger = new Logger<SegmentIDImpl>(loggerFactory);
            idgen = new SegmentIDImpl(logger, dal);

            await idgen.InitAsync();
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
    }
}