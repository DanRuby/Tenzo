using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using tEngine.Helpers;

namespace Oscill {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private void Application_Exit( object sender, ExitEventArgs e ) {
            AppSettings.Save();
        }

        private void Application_Startup( object sender, StartupEventArgs e ) {
            AppSettings.Init();
        }
    }
}
