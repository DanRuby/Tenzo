using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using tEngine.DataModel;
using tEngine.Helpers;
using tEngine.Recorder;
using tEngine.TMeter;

namespace TenzoMeterGUI.View
{
    /// <summary>
    /// Interaction logic for Markers.xaml
    /// </summary>
    public partial class Markers : Window
    {
        private const int INTEGRATE_COUNT = 15;
        private static Device mDevice;
        private static Markers mWindow;
        private Queue<int> mLeft = new Queue<int>();
        private object mLock = new object();
        private Queue<int> mRight = new Queue<int>();
        private DispatcherTimer mTimerDraw;

        public static bool IsWindow
        {
            get { return mWindow != null; }
        }

        public int LeftHand
        {
            get
            {
                lock (mLock)
                {
                    if (mLeft.Count == 0)
                        return 0;
                    return (int)mLeft.Average();
                }
            }
            private set
            {
                lock (mLock)
                {
                    mLeft.Enqueue(value);
                    if (mLeft.Count > INTEGRATE_COUNT)
                        mLeft.Dequeue();
                }
            }
        }

        public int RightHand
        {
            get
            {
                lock (mLock)
                {
                    if (mRight.Count == 0)
                        return 0;
                    return (int)mRight.Average();
                }
            }
            private set
            {
                lock (mLock)
                {
                    mRight.Enqueue(value);
                    if (mRight.Count > INTEGRATE_COUNT)
                        mRight.Dequeue();
                }
            }
        }

        public static void CloseWindow()
        {
            if (mWindow != null)
                mWindow.Close();
        }

        public void FreeWindow()
        {
            mWindow = null;
        }
        public static Markers GetWindow()
        {
            if (!IsWindow)
            {
                mWindow = new Markers();
            }
            return mWindow;
        }

        public void UpdateSettings()
        {
            MarkersArea.UpdateArea();
        }

        internal Markers()
        {
            mDevice = Device.GetDevice(Constants.DEVICE_ID);
            mDevice.Stop();

            InitializeComponent();
            tgb.IsChecked = mDevice.DemoMode;

            WindowManager.UpdateWindowPos(GetType().Name, this);
            mTimerDraw = new DispatcherTimer();
            mTimerDraw.Tick += TimerDrawOnTick;
            mTimerDraw.Interval = new TimeSpan(0, 0, 0, 0, 1000 / 25);
            mTimerDraw.Start();

            mDevice.AddListener(HandCallBack);
        }

        /// <summary>
        /// отрисовка
        /// </summary>
        /// <param name="requestID"></param>
        /// <param name="hand1"></param>
        /// <param name="hand2"></param>
        private void HandCallBack(ushort requestID, Hand hand1, Hand hand2)
        {
            LeftHand = (int)hand1.Const.Average(s => s);
            RightHand = (int)hand2.Const.Average(s => s);
            return;
        }

        private void TimerDrawOnTick(object sender, EventArgs eventArgs)
        {
            MarkersArea.DrawPart(LeftHand, RightHand);
        }

        public void ReDraw()
        {
            MarkersArea.DrawPart(LeftHand, RightHand);
        }

        private void Window_OnClosing(object sender, CancelEventArgs e)
        {
            mDevice.RemoveListener(HandCallBack);
            mDevice.Stop();
            mTimerDraw.Stop();
            mWindow = null;
            WindowManager.SaveWindowPos(GetType().Name, this);
        }

        private void Markers_OnLoaded(object sender, RoutedEventArgs e)
        {
            mDevice.Start();
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton cb = (sender as ToggleButton);
            if (cb == null)
                return;
            mDevice.DemoMode = cb.IsChecked == true;
        }
    }
}