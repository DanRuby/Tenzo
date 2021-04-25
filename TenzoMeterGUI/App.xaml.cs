using System.Windows;
using tEngine.Helpers;

namespace TenzoMeterGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {

        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            AppSettings.Init();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            AppSettings.Save();
        }
    }
}