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
    public partial class Markers : Window
    {
        private const int INTEGRATE_COUNT = 15;
        private const int TIMER_UPDATE_INTERVAL= 40;
        private static Device mDevice;
        private static Markers mWindow;
        private DispatcherTimer dispetcherTimer;
        private Queue<int> mLeft = new Queue<int>();
        private Queue<int> mRight = new Queue<int>();
        private object mLock = new object();

        /// <summary>
        /// Среднее последних значений  усилия левой руки
        /// </summary>
        private int LeftHand
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
            set
            {
                lock (mLock)
                {
                    mLeft.Enqueue(value);
                    if (mLeft.Count > INTEGRATE_COUNT)
                        mLeft.Dequeue();
                }
            }
        }

        /// <summary>
        /// Среднее последних значений усилия правой руки
        /// </summary>
        private int RightHand
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
           set
            {
                lock (mLock)
                {
                    mRight.Enqueue(value);
                    if (mRight.Count > INTEGRATE_COUNT)
                        mRight.Dequeue();
                }
            }
        }

        public static bool WindowNotNull => mWindow != null;

        /// <summary>
        /// Закрыть окно
        /// </summary>
        public static void CloseWindow()
        {
            if (mWindow != null)
                mWindow.Close();
        }

        /// <summary>
        /// Получить ссылку на окно
        /// </summary>
        /// <returns></returns>
        public static Markers GetWindow()
        {
            if (!WindowNotNull)
                mWindow = new Markers();
            return mWindow;
        }

        /// <summary>
        /// Обновить настройки маркеров
        /// </summary>
        public void UpdateSettings() => UiMarkers.UpdateArea();

        internal Markers()
        {
            mDevice = Device.GetDevice(Constants.DEVICE_ID);
            mDevice.Stop();

            InitializeComponent();
            //Тестовая кнопка
            //tgb.IsChecked = mDevice.DemoMode;

            WindowManager.UpdateWindowPos(GetType().Name, this);
            dispetcherTimer = new DispatcherTimer();
            dispetcherTimer.Tick += TimerDrawOnTick;
            dispetcherTimer.Interval = new TimeSpan(0, 0, 0, 0, TIMER_UPDATE_INTERVAL);
            dispetcherTimer.Start();

            mDevice.AddListener(HandCallBack);
        }


        /// <summary>
        /// Перерисовать маркеры
        /// </summary>
        public void ReDraw() =>UiMarkers.DrawMarkers(LeftHand, RightHand);

        /// <summary>
        /// Получение новых значений усилия
        /// </summary>
        /// <param name="requestID"></param>
        /// <param name="hand1"></param>
        /// <param name="hand2"></param>
        private void HandCallBack(ushort requestID, HandRawData hand1, HandRawData hand2)
        {
            LeftHand = (int)hand1.Constant.Average(s => s);
            RightHand = (int)hand2.Constant.Average(s => s);
        }

        /// <summary>
        /// Метод вызываемый по изменению таймера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void TimerDrawOnTick(object sender, EventArgs eventArgs) => UiMarkers.DrawMarkers(LeftHand, RightHand);

        /// <summary>
        /// Метод вызываемый при закрытии метода
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_OnClosing(object sender, CancelEventArgs e)
        {
            mDevice.RemoveListener(HandCallBack);
            mDevice.Stop();
            dispetcherTimer.Stop();
            mWindow = null;
            WindowManager.SaveWindowPos(GetType().Name, this);
        }

        /// <summary>
        /// Метод вызываемый при открытии формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Markers_OnLoaded(object sender, RoutedEventArgs e) => mDevice.Start();

        //Тестовая кнопка
        /*
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton cb = (sender as ToggleButton);
            if (cb == null)
                return;
            mDevice.DemoMode = cb.IsChecked == true;
        }*/
    }
}