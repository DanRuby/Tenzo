using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using tEngine.Helpers;
using tEngine.MVVM;
using tEngine.TMeter;
using tEngine.TMeter.DataModel;

namespace TenzoMeterGUI.View.old {
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    // BUG не хватает вывода окна на верхний слой при переключениях
    public partial class StartWindow : Window {
        public StartWindow() {
            AppSettings.Init( AppSettings.Project.Meter );
            InitializeComponent();
            WindowManager.UpdateWindowPos( this.GetType().Name, this );

            DataContext = new StartWindowVM() {Parent = this};
        }

        private void Window_OnClosing( object sender, CancelEventArgs e ) {
            WindowManager.SaveWindowPos( this.GetType().Name, this );
            AppSettings.Save();
        }
    }

    public class StartWindowVM : Observed<StartWindowVM> {
        public Command CMDWorkMsm { get; private set; }
        public Command CMDWorkUser { get; private set; }
        public Command CMDWorkUserTest { get; private set; }

        public StartWindowVM() {
            CMDWorkMsm = new Command( WorkMsm );
            CMDWorkUser = new Command( WorkUser );
            CMDWorkUserTest = new Command( WorkUserTest );
        }

        private void WorkMsm( object param ) {
            bool create;
            create = !Boolean.TryParse( param as string, out create ) || create;
            WorkMsm( create );
        }

        private void WorkMsm( bool create = false ) {}

        private void WorkUser( bool create = false ) {
            User workUser = null;
            if( create ) {
                var cuDialog = new UserInfo();
                cuDialog.EditMode = true;
                if( cuDialog.ShowDialog() == true ) {
                    if( cuDialog.Result != null ) {
                        workUser = cuDialog.Result;
                        workUser.Save();
                    }
                }
            } else {
                var ouDialog = new OpenFileDialog();
                ouDialog.Filter = string.Format( "*{0}|*{0}", Constants.USER_EXT );
                ouDialog.RestoreDirectory = true;
                var initPath = AppSettings.GetValue( User.FOLDER_KEY, Constants.AppDataFolder );
                ouDialog.InitialDirectory = initPath + @"\";
                if( ouDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ) {
                    if( User.Open( ouDialog.FileName, out workUser ) == false )
                        workUser = null;
                }
            }
            if( Parent != null ) Parent.Hide();
            if( workUser != null ) {
                var wnd = new UserWork();
                wnd.Init( workUser );
                wnd.ShowDialog();
            }
            if( Parent != null ) Parent.Show();
        }

        private void WorkUser( object param ) {
            bool create;
            create = !Boolean.TryParse( param as string, out create ) || create;
            WorkUser( create );
        }

        private void WorkUserTest() {
            var wnd = new UserWork();
            wnd.Init( User.GetTestUser() );
            wnd.ShowDialog();
            if( Parent != null ) Parent.Show();
        }
    }
}