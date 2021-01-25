using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using tEngine.Helpers;
using tEngine.MVVM;
using tEngine.TActual.DataModel;

namespace TenzoActualGUI.View.old {
    /// <summary>
    /// Interaction logic for ResultWindow.xaml
    /// </summary>
    public partial class ResultWindow : Window {
        private ResultWindowVM mDataContext;

        public ResultWindow() {
            InitializeComponent();
            WindowManager.UpdateWindowPos( this.GetType().Name, this );
            mDataContext = new ResultWindowVM() {Parent = this};
            DataContext = mDataContext;
        }

        public void Init( PlayList playList ) {
            mDataContext.PlayList = playList;
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

    public class ResultWindowVM : Observed<ResultWindowVM> {
        private PlayList mPlayList;

        public PlayList PlayList {
            get { return mPlayList; }
            set {
                mPlayList = value ?? new PlayList();
                NotifyPropertyChanged( m => m.Slides );
            }
        }

        public ReadOnlyCollection<Slide> Slides {
            get { return PlayList.Slides; }
        }

        public ResultWindowVM() {
            PlayList = new PlayList();
        }
    }
}