using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using tEngine.Helpers;
using tEngine.MVVM;
using tEngine.TActual.DataModel;

namespace TenzoActualGUI.View {
    /// <summary>
    /// Interaction logic for testpl.xaml
    /// </summary>
    public partial class testpl : Window {
        private testplVM mDataContext;
        public testpl() {
            AppSettings.Init( AppSettings.Project.Actual );
            InitializeComponent();
            WindowManager.UpdateWindowPos( this.GetType().Name, this );
            mDataContext = new testplVM() {Parent = this};
            DataContext = mDataContext;
        }

        private void Window_OnClosing( object sender, CancelEventArgs e ) {
            if ( mDataContext != null ) {
                try {
                    DialogResult = mDataContext.DialogResult;
                } catch { /*если окно не диалог - вылетит исключение, ну и пусть*/ }
            }
            WindowManager.SaveWindowPos( this.GetType().Name, this );
            AppSettings.Save();
        }
    }


    public class testplVM : Observed<testplVM> {
        PlayList PlayList { get; set; }

        public string Check {
            get { return "All is right"; }
        }
        public testplVM() {
            PlayList = new PlayList();
        }
    }
}
