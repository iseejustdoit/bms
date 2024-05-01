using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace WpfLeafTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {/// <summary>
     /// 获取当前 App 实例
     /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// 获取存放应用服务的容器
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        public App()
        {
            ServiceProvider = ConfigureServices();
        }

        /// <summary>
        /// 配置应用的服务
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection()
                .AddTransient<ITextService, TextService>()
                .AddSingleton<MainWindow>();

            return serviceCollection.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }

}
