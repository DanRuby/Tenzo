using System;
using System.Collections.Generic;
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
using System.Windows.Threading;
using tEngine.DataModel;
using tEngine.Recorder;

namespace TMSingleMeasurement.View {
    /// <summary>
    /// Interaction logic for Markers.xaml
    /// </summary>
    public partial class Markers : Window {
        private const int INTEGRATE_COUNT = 25;
        private static Device mDevice = Device.GetDevice( 0 );
        private static List<Markers> mMWindow = new List<Markers>();
        private Queue<int> mLeft = new Queue<int>();
        private object mLock = new object();
        private Queue<int> mRight = new Queue<int>();
        private DispatcherTimer mTimerDraw;

        public int LeftHand {
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

        public static Markers MWindow {
            get {
                if( mMWindow.Any() )
                    return mMWindow[0];
                return null;
            }
        }

        public int RightHand {
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

        public static void CloseAll() {
            mMWindow.ForEach( m => m.Close() );
            mMWindow.Clear();
        }

        public static Markers CreateWindow() {
            if( mMWindow.Count == 0 ) {
                var wnd = new Markers();
                mMWindow.Add( wnd );
            }
            return mMWindow[0];
        }

        public void UpdateSettings() {
            MarkersArea.UpdateArea();
        }

        internal Markers() {
            InitializeComponent();
            mTimerDraw = new DispatcherTimer();
            mTimerDraw.Tick += TimerDrawOnTick;
            mTimerDraw.Interval = new TimeSpan( 0, 0, 0, 0, 1000/25 );
            mTimerDraw.Start();

            mDevice.AddListener( HandCallBack );
        }

        /// <summary>
        /// отрисовка
        /// </summary>
        /// <param name="requestID"></param>
        /// <param name="hand1"></param>
        /// <param name="hand2"></param>
        private void HandCallBack( ushort requestID, Hand hand1, Hand hand2 ) {
            LeftHand = (int) hand1.Const.Average( s => s );
            RightHand = (int) hand2.Const.Average( s => s );
            return;
        }

        private void Window_OnClosing( object sender, CancelEventArgs e ) {
            mDevice.RemoveListener( HandCallBack );
            mMWindow.Clear();
        }

        private void TimerDrawOnTick( object sender, EventArgs eventArgs ) {
            MarkersArea.DrawPart( LeftHand, RightHand );
        }
    }
}