using System.Windows;

namespace WpfLeafTest
{
    /// <summary>
    /// Test.xaml 的交互逻辑
    /// </summary>
    public partial class Test : Window
    {
        private readonly ITextService textService;

        public Test(ITextService textService)
        {
            InitializeComponent();
            this.textService = textService;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            textService.Plus();
        }
    }
}
