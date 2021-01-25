using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using tEngine.DataModel;
using tEngine.MVVM;

namespace TenzoMeterGUI.View.old {
    /// <summary>
    /// Interaction logic for SpectrumProgress.xaml
    /// </summary>
    public partial class SpectrumProgress : Window {
        private SpectrumProgressVM mDataContext;

        public SpectrumProgress() {
            InitializeComponent();

            mDataContext = new SpectrumProgressVM() {Parent = this};
            DataContext = mDataContext;
        }

        public bool? Show( TData[] actions ) {
            mDataContext.Data = actions;
            mDataContext.Start();
            return this.ShowDialog();
        }

        private void Window_OnClosing( object sender, CancelEventArgs e ) {
            if( mDataContext != null ) {
                mDataContext.PreClosed();
                try {
                    DialogResult = mDataContext.DialogResult;
                } catch {
                    /*если окно не диалог - вылетит исключение, ну и пусть*/
                }
            }
        }
    }

    public class SpectrumProgressVM : Observed<SpectrumProgressVM> {
        private readonly CancellationTokenSource mCts = new CancellationTokenSource();
        private double mCurrentProgress;
        private double mFullProgress = 0;
        private List<Guid> mIdList = new List<Guid>();
        private bool mInProcess = false;
        private TData[] mTData = new TData[] {};
        public Command CMDCancel { get; private set; }

        public double CurrentProgress {
            get { return mCurrentProgress; }
            set {
                mCurrentProgress = value;
                NotifyPropertyChanged( m => m.CurrentProgress );
            }
        }

        public TData[] Data {
            get { return mTData; }
            set { mTData = value; }
        }

        public double FullProgress {
            get { return mFullProgress; }
            set {
                mFullProgress = value;
                NotifyPropertyChanged( m => m.FullProgress );
            }
        }

        public SpectrumProgressVM() {
            CMDCancel = new Command( Cancel );
        }

        public void PreClosed() {}

        public void Start() {
            CurrentProgress = 0;
            FullProgress = 0;
            if( Data == null ) {
                EndDialog( dialogResult: false );
                return;
            }
            Task.Factory.StartNew( ( param ) => Calculate( (CancellationToken) param ), mCts.Token, mCts.Token );
        }

        private void CalcPercents( int index, double p1, double p2 ) {
            if( Data == null ) return;
            var count = Data.Count();
            CurrentProgress = (p1 + p2)/2.0;
            FullProgress = 100.0*index/count;
        }

        //todo следует проверить на больших массивах
        private void Calculate( CancellationToken cts ) {
            var queue = new Queue<TData>( Data );
            int i = 0;
            while( queue.Count > 0 && cts.IsCancellationRequested == false ) {
                var data = queue.Dequeue();
                mInProcess = true;
                var id = data.SpectrumAnalys( ( p1, p2 ) => CalcPercents( i, p1, p2 ), ( data1, b ) => {
                    if( b == true ) {
                        //todo обработать ошибку вычисления спектра
                    }
                    mInProcess = false;
                } );
                mIdList.Add( id );
                while( mInProcess == true && cts.IsCancellationRequested == false ) {
                    Thread.Sleep( 20 );
                }
                i++;
                CalcPercents( i, 100, 100 );
            }
            var d = Parent.Dispatcher.BeginInvoke( DispatcherPriority.Normal, new Action<bool?>( EndDialog ),
                !mCts.IsCancellationRequested );

            //EndDialog( dialogResult: !mCts.IsCancellationRequested );
        }

        private void Cancel() {
            mCts.Cancel();
            mIdList.ForEach( guid => {
                TData.CancelCalc();
            } );
        }
    }
}