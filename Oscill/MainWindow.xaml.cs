using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using tEngine.DataModel;
using tEngine.Markers;
using tEngine.Recorder;

namespace Oscill
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private const int INTEGRATE_COUNT = 25;

        public static readonly DependencyProperty LogProperty = DependencyProperty.Register( "Log", typeof( string ),
            typeof( MainWindow ), new PropertyMetadata( default(string) ) );

        private static Device mDevice;
        private static List<MainWindow> mMWindow = new List<MainWindow>();
        private TData Data = new TData();
        private bool mCanClear = false;

        private LineSeries mConstSeries_Left = new LineSeries() {
            Smooth = false,
            LineStyle = LineStyle.Solid,
            StrokeThickness = 2
        };

        private LineSeries mConstSeries_Right = new LineSeries() {
            Smooth = false,
            LineStyle = LineStyle.Solid,
            StrokeThickness = 2
        };

        private int mDrawCount = 0;
        private double mDrawPeriod = 1000/5.0;
        private Queue<int> mLeft = new Queue<int>();
        private object mLock = new object();
        private Queue<int> mRight = new Queue<int>();
        private DispatcherTimer mTimerDraw;
        private DispatcherTimer mTimerGraphics;

        private LineSeries mTremorSeries_Left = new LineSeries() {
            Smooth = false,
            LineStyle = LineStyle.Solid,
            StrokeThickness = 2
        };

        private LineSeries mTremorSeries_Right = new LineSeries() {
            Smooth = false,
            LineStyle = LineStyle.Solid,
            StrokeThickness = 2
        };

        public bool DemoMode {
            get { return mDevice.DemoMode; }
            set {
                mDevice.DemoMode = value;
                Data.Clear();
            }
        }

        public int LeftData {
            get {
                lock( mLock ) {
                    if( mLeft.Count == 0 )
                        return 0;
                    return (int) mLeft.Average();
                }
            }
            private set {
                lock( mLock ) {
                    mLeft.Enqueue( value );
                    if( mLeft.Count > INTEGRATE_COUNT )
                        mLeft.Dequeue();
                }
            }
        }

        public string Log {
            get { return (string) GetValue( LogProperty ); }
            set { SetValue( LogProperty, value ); }
        }

        public static MainWindow MWindow {
            get {
                if( mMWindow.Any() )
                    return mMWindow[0];
                return null;
            }
        }

        public int RightData {
            get {
                lock( mLock ) {
                    if( mRight.Count == 0 )
                        return 0;
                    return (int) mRight.Average();
                }
            }
            private set {
                lock( mLock ) {
                    mRight.Enqueue( value );
                    if( mRight.Count > INTEGRATE_COUNT )
                        mRight.Dequeue();
                }
            }
        }

        public MainWindow() {
            InitializeComponent();
            DataContext = this;
            OxyConst.Model = new PlotModel() {
                Title = OxyConst.Title,
                Series = {mConstSeries_Left, mConstSeries_Right},
                Axes = {
                    new LinearAxis() {
                        Position = AxisPosition.Left,
                        IsZoomEnabled = false,
                        IsPanEnabled = false,
                        Minimum = MarkersArea.Minimum,
                        Maximum = MarkersArea.Maximum
                    },
                    new LinearAxis() {
                        Position = AxisPosition.Bottom,
                        IsZoomEnabled = false,
                        IsPanEnabled = false,
                        IsAxisVisible = false
                    }
                }
            };
            OxyTremor.Model = new PlotModel() {
                Title = OxyTremor.Title,
                Series = {mTremorSeries_Left, mTremorSeries_Right},
                Axes = {
                    new LinearAxis() {
                        Position = AxisPosition.Left,
                        IsZoomEnabled = false,
                        IsPanEnabled = false,
                    },
                    new LinearAxis() {
                        Position = AxisPosition.Bottom,
                        IsZoomEnabled = false,
                        IsPanEnabled = false,
                        IsAxisVisible = false
                    }
                }
            };

            mDevice = Device.GetDevice( 0 );
            mDevice.Stop();
            mDevice.DemoMode = true;


            var device = new DispatcherTimer();
            device.Tick += ( sender, args ) => {
                AddLog( mDevice.DeviceState.ToString() );
            };
            device.Interval = new TimeSpan( 0, 0, 0, 3 );
            device.Start();


            mTimerDraw = new DispatcherTimer();
            mTimerDraw.Tick += TimerDrawOnTick;
            mTimerDraw.Interval = new TimeSpan( 0, 0, 0, 0, 1000/25 );
            mTimerDraw.Start();

            mTimerGraphics = new DispatcherTimer();
            mTimerGraphics.Tick += TimerGraphicsOnTick;
            mTimerGraphics.Interval = new TimeSpan( 0, 0, 0, 0, (int) mDrawPeriod );
            mTimerGraphics.Stop();

            mDevice.AddListener( HandCallBack );
        }

        public void AddLog( string msg ) {
            var time = DateTime.Now.ToString( "G" );
            Log += string.Format( "{0} - {1}{2}", time, msg, Environment.NewLine );
            LogBox.ScrollToEnd();
        }

        public static void CloseAll() {
            mMWindow.ForEach( m => m.Close() );
            mMWindow.Clear();
        }

        public static MainWindow CreateWindow() {
            if( mMWindow.Count == 0 ) {
                var wnd = new MainWindow();
                mMWindow.Add( wnd );
            }
            return mMWindow[0];
        }

        public void UpdateSettings() {
            MarkersArea.UpdateArea();
        }

        private void Button_Click( object sender, RoutedEventArgs e ) {
            if( new MarkersSet().ShowDialog() == true ) {
                UpdateSettings();
                (OxyConst.Model as PlotModel).Axes[0].Minimum = MarkersArea.Minimum;
                (OxyConst.Model as PlotModel).Axes[0].Maximum = MarkersArea.Maximum;
            }
        }

        /// <summary>
        /// отрисовка
        /// </summary>
        /// <param name="requestID"></param>
        /// <param name="hand1"></param>
        /// <param name="hand2"></param>
        private void HandCallBack( ushort requestID, Hand hand1, Hand hand2 ) {
            LeftData = (int) hand1.Const.Average( s => s );
            RightData = (int) hand2.Const.Average( s => s );
            if ( mCanClear ) {
                Data.Clear();
                mCanClear = false;
            }
            if(mTimerGraphics.IsEnabled)
                Data.AddHands( hand1, hand2 );
        }

        private void MainWindow_OnLoaded( object sender, RoutedEventArgs e ) {
            mDevice.Start();
        }

        private void TimerDrawOnTick( object sender, EventArgs eventArgs ) {
            MarkersArea.DrawPart( LeftData, RightData );
        }

        private void TimerGraphicsOnTick( object sender, EventArgs eventArgs ) {
            mCanClear = false;
            var leftConst = Data.GetConstBase( Hands.Left );
            var leftTremor = Data.GetTremorBase( Hands.Left );
            var rightConst = Data.GetConstBase( Hands.Right );
            var rightTremor = Data.GetTremorBase( Hands.Right );

            if ( mDrawCount * mDrawPeriod > 5 * 1000 ) {
                mConstSeries_Left.Points.Clear();
                mConstSeries_Right.Points.Clear();
                mTremorSeries_Left.Points.Clear();
                mTremorSeries_Right.Points.Clear();
                mDrawCount = 0;
            }

            var d = mConstSeries_Left.Points.Count;
            mConstSeries_Left.Points.AddRange( leftConst.Select( ( s, i ) => new DataPoint( d + i, s ) ) );
            mConstSeries_Right.Points.AddRange( rightConst.Select( ( s, i ) => new DataPoint( d + i, s ) ) );
            (OxyConst.Model as PlotModel).InvalidatePlot( true );

            mTremorSeries_Left.Points.AddRange( leftTremor.Select( ( s, i ) => new DataPoint( d + i, s ) ) );
            mTremorSeries_Right.Points.AddRange( rightTremor.Select( ( s, i ) => new DataPoint( d + i, s ) ) );
            (OxyTremor.Model as PlotModel).InvalidatePlot( true );

            mCanClear = true;
            mDrawCount++;
        }

        private void Window_OnClosing( object sender, CancelEventArgs e ) {
            mDevice.RemoveListener( HandCallBack );
            mMWindow.Clear();
        }

        private void ToggleButton_Checked( object sender, RoutedEventArgs e ) {
            if( (sender as ToggleButton).IsChecked == true ) {

                mConstSeries_Left.Points.Clear();
                mConstSeries_Right.Points.Clear();
                mTremorSeries_Left.Points.Clear();
                mTremorSeries_Right.Points.Clear();
                Data.Clear();
                mTimerGraphics.Start();
            } else {
                mTimerGraphics.Stop();
            }
        }
    }
}