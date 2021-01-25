using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

        public void SetMsm( Measurement msm ) {
            mDataContext.Msm = msm;
        }

        private void Window_OnClosing( object sender, CancelEventArgs e ) {
            if( mDataContext != null ) {
                try {
                    DialogResult = mDataContext.DialogResult;
                } catch( Exception  ) {
                    //Debug.Assert( false, ex.Message );
                }
            }
            WindowManager.SaveWindowPos( this.GetType().Name, this );
        }

        private void FrameworkElement_OnTargetUpdated( object sender, DataTransferEventArgs e ) {
            mDataContext.UpdateQueue --;
        }
    }

    public enum EShowMode {
        Const,
        Tremor,
        Spectrum,
        Correlation
    }

    public class ResultWindowVM : Observed<ResultWindowVM> {
        private double mHzLower;
        private double mHzUpper;
        private double mResolution;
        private EShowMode mShowMode;
        private Measurement mMsm;
        private int mUpdateQueue;
        private bool mIsBusyNow;
        public Command CMDSelectSlide { get; private set; }
        public Command CMDUpdateGraphics { get; private set; }

        public double HzLower {
            get { return mHzLower; }
            set {
                mHzLower = value;
                NotifyPropertyChanged( m => m.HzLower );
            }
        }

        public double HzLowerToDraw {
            get { return HzLower; }
        }

        public double HzUpper {
            get { return mHzUpper; }
            set {
                mHzUpper = value;
                NotifyPropertyChanged( m => m.HzUpper );
            }
        }

        public int UpdateQueue {
            get { return mUpdateQueue; }
            set {
                mUpdateQueue = value;
                IsBusyNow = mUpdateQueue > 0;
            }
        }

        public double HzUpperToDraw {
            get { return HzUpper; }
        }

        public Measurement Msm {
            get { return mMsm; }
            set {
                mMsm = value;
                UpdateQueue = mMsm.PlayList.Slides.Count * 2;
            }
        }

        public double Resolution {
            get { return mResolution; }
            set {
                mResolution = value;
                NotifyPropertyChanged( m => m.Resolution );
            }
        }

        public double ResolutionToDraw {
            get { return Resolution; }
        }

        public EShowMode ShowMode {
            get { return mShowMode; }
            set {
                mShowMode = value;
                NotifyPropertyChanged( m => m.ShowMode );
            }
        }

        public EShowMode ShowModeToDraw {
            get { return ShowMode; }
        }

        public ResultWindowVM() {
            CMDSelectSlide = new Command( CMDSelectSlide_Func );
            CMDUpdateGraphics = new Command( CMDUpdateGraphics_Func );
            
            HzLower = 0;
            HzUpper = 50;
            Resolution = 10;
            if( IsDesignMode ) {
                Msm = Measurement.CreateTestMsm( 2 );
            }
        }

        private void CMDSelectSlide_Func() {
            throw new NotImplementedException();
        }

        public ObservableCollection<Slide> Slides {
            get { return new ObservableCollection<Slide>(Msm.PlayList.Slides); }
        }

        public bool IsBusyNow {
            get { return mIsBusyNow; }
            set {
                mIsBusyNow = value; 
                NotifyPropertyChanged( m=>m.IsBusyNow );
            }
        }

        private void CMDUpdateGraphics_Func( object param ) {
            if( param is ListView ) {
                UpdateQueue = Msm.PlayList.Slides.Count * 2;
                NotifyPropertyChanged( m=>m.Slides );
            }
        }
    }
}