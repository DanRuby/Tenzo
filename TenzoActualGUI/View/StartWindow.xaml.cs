using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Win32;
using tEngine.Helpers;
using tEngine.MVVM;
using tEngine.TActual;
using tEngine.TActual.DataModel;
using TenzoActualGUI.ViewModel;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;


namespace TenzoActualGUI.View {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StartWindow : Window {
        private StartWindowVM mDataContext;

        public StartWindow() {
            try {
                InitializeComponent();
                WindowManager.UpdateWindowPos( this.GetType().Name, this );
                mDataContext = new StartWindowVM() {Parent = this};
                DataContext = mDataContext;
            } catch( Exception  ) {
                //ex = ex;
            }
        }

        private void Window_OnClosing( object sender, CancelEventArgs e ) {
            if( mDataContext != null ) {
                try {
                    DialogResult = mDataContext.DialogResult;
                } catch {
                    /*если окно не диалог - вылетит исключение, ну и пусть*/
                }
            }
            WindowManager.SaveWindowPos( this.GetType().Name, this );
        }
    }

    public class StartWindowVM : Observed<StartWindowVM> {
        public Command CMDOneMsmShow { get; private set; }
        public Command CMDOneMsmTest { get; private set; }
        public Command CMDSlideCreatorShow { get; private set; }

        public StartWindowVM() {
            CMDSlideCreatorShow = new Command( SlideCreatorShow );
            CMDOneMsmShow = new Command( OneMsmShow );
            CMDOneMsmTest = new Command( OneMsmTest );
        }

        private void OneMsmShow( object param ) {
            bool create;
            create = !Boolean.TryParse( param as string, out create ) || create;
            OneMsmShow( create );
        }

        private void OneMsmShow( bool create ) {
            try {
                if( Parent != null ) Parent.Hide();

                var wnd = new MsmMaster();

                if( create ) {
                    var msm = new Measurement();
                    msm.PlayList.IsNotSaveChanges = false;
                    wnd.SetMsm( msm );
                } else {
                    wnd.LoadWithOpen = true;
                }
                wnd.ShowDialog();
            } catch(Exception ex) {
                Debug.Assert( false, ex.ToString() );
            } finally {
                if( Parent != null ) Parent.Show();
            }
        }

        private void OneMsmTest() {
            //PlayList playList = null;
            if( Parent != null )
                Parent.Hide();

            var wnd = new MsmMaster();
            (wnd.DataContext as MsmMasterVM).TestMode();
            wnd.ShowDialog();
            if( Parent != null )
                Parent.Show();
        }

        private void SlideCreatorShow( object param ) {
            bool create;
            create = !Boolean.TryParse( param as string, out create ) || create;
            SlideCreatorShow( create );
        }

        private void SlideCreatorShow( bool create ) {
            PlayList playList = null;
            if( create ) {
                playList = new PlayList();
            } else {
                var file = SlideCreatorVM.OpenDialog();
                if( file == null ) return;
                PlayList.Open( file, out playList );
            }
            if( playList != null ) {
                if( Parent != null ) Parent.Hide();

                var wnd = new SlideCreator {PlayList = playList};
                wnd.ShowDialog();
            }
            if( Parent != null ) Parent.Show();
        }
    }
}