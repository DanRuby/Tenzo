﻿using System.Windows;
using tEngine.Helpers;
using tEngine.Recorder;
using tEngine.TMeter;

namespace TenzoMeterGUI {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public App() {
            
        }

        private void App_OnStartup( object sender, StartupEventArgs e ) {
            AppSettings.Init( AppSettings.Project.Meter );
        }

        private void Application_Exit( object sender, ExitEventArgs e ) {
            AppSettings.Save();
        }
    }
}