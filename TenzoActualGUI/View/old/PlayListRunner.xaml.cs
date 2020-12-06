using System.ComponentModel;
using System.Windows;
using tEngine.Helpers;
using tEngine.MVVM;

namespace TenzoActualGUI.View.old {
    /// <summary>
    /// Interaction logic for PlayListRunner.xaml
    /// </summary>
    public partial class PlayListRunner : Window {
        private PlayListRunnerVM mDataContext;

        public PlayListRunner() {
            InitializeComponent();
            WindowManager.UpdateWindowPos( this.GetType().Name, this );
            mDataContext = new PlayListRunnerVM() {Parent = this};
            DataContext = mDataContext;
        }

        private void Window_OnClosing( object sender, CancelEventArgs e ) {
            if ( mDataContext != null ) {
                try {
                    DialogResult = mDataContext.DialogResult;
                } catch { /*если окно не диалог - вылетит исключение, ну и пусть*/ }
            }
            WindowManager.SaveWindowPos( this.GetType().Name, this );
        }
    }

    public class PlayListRunnerVM : Observed<PlayListRunnerVM> {}
}