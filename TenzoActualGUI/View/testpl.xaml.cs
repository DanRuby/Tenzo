using System.ComponentModel;
using System.Windows;
using tEngine.Helpers;
using tEngine.MVVM;
using tEngine.TActual.DataModel;

namespace TenzoActualGUI.View
{
    /// <summary>
    /// Interaction logic for testpl.xaml
    /// </summary>
    public partial class testpl : Window
    {
        private testplVM mDataContext;
        public testpl()
        {
            AppSettings.Init(AppSettings.Project.Actual);
            InitializeComponent();
            WindowManager.UpdateWindowPos(GetType().Name, this);
            mDataContext = new testplVM() { Parent = this };
            DataContext = mDataContext;
        }

        private void Window_OnClosing(object sender, CancelEventArgs e)
        {
            if (mDataContext != null)
            {
                try
                {
                    DialogResult = mDataContext.DialogResult;
                }
                catch { /*если окно не диалог - вылетит исключение, ну и пусть*/ }
            }
            WindowManager.SaveWindowPos(GetType().Name, this);
            AppSettings.Save();
        }
    }


    public class testplVM : Observed<testplVM>
    {
        PlayList PlayList { get; set; }

        public string Check
        {
            get { return "All is right"; }
        }
        public testplVM()
        {
            PlayList = new PlayList();
        }
    }
}
