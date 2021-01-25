using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using tEngine.DataModel;
using tEngine.Helpers;
using tEngine.MVVM;
using tEngine.Recorder;

namespace Oscill
{
    /// <summary>
    /// Interaction logic for Simple.xaml
    /// </summary>
    public partial class Simple : Window
    {
        private SimpleVM mDataContext;

        public Simple()
        {
            InitializeComponent();
            WindowManager.UpdateWindowPos(this.GetType().Name, this);
            mDataContext = new SimpleVM() { Parent = this };
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
                catch
                {
                    /*если окно не диалог - вылетит исключение, ну и пусть*/
                }
            }
            WindowManager.SaveWindowPos(this.GetType().Name, this);
        }
    }

    public class SimpleVM : Observed<SimpleVM>
    {
        private const int INTEGRATE_COUNT = 50;
        private Queue<int> mConst1 = new Queue<int>();
        private Queue<int> mConst2 = new Queue<int>();
        private Device mDevice = Device.CreateDevice(0);
        private bool mIsRun;
        private object mLock = new object();
        private DispatcherTimer mTimer;
        private Queue<int> mTremor1 = new Queue<int>();
        private Queue<int> mTremor2 = new Queue<int>();

        public int Const1
        {
            get
            {
                lock (mLock)
                {
                    if (mConst1.Count == 0)
                        return 0;
                    return (int)mConst1.Average();
                }
            }
            private set
            {
                lock (mLock)
                {
                    mConst1.Enqueue(value);
                    if (mConst1.Count > INTEGRATE_COUNT)
                        mConst1.Dequeue();
                }
            }
        }

        public int Const2
        {
            get
            {
                lock (mLock)
                {
                    if (mConst2.Count == 0)
                        return 0;
                    return (int)mConst2.Average();
                }
            }
            private set
            {
                lock (mLock)
                {
                    mConst2.Enqueue(value);
                    if (mConst2.Count > INTEGRATE_COUNT)
                        mConst2.Dequeue();
                }
            }
        }

        public bool IsRun
        {
            get { return mIsRun; }
            set
            {
                mIsRun = value;
                if (mIsRun) mDevice.Start();
                else mDevice.Stop();
                NotifyPropertyChanged(m => m.IsRun);
            }
        }

        public string State
        {
            get
            {
                if (mDevice.DeviceState == DeviceStates.AllRight)
                    return "Есть связь с устройством";
                if (mDevice.DeviceState == DeviceStates.DllNotFound)
                    return "Не найдена библиотека TenzoDevice.dll";
                if (mDevice.DeviceState == DeviceStates.DeviceNotFound)
                    return "Устройство не найдено";
                return mDevice.DeviceState.ToString();
            }
        }

        public int Tremor1
        {
            get
            {
                lock (mLock)
                {
                    if (mTremor1.Count == 0)
                        return 0;
                    return (int)mTremor1.Average();
                }
            }
            private set
            {
                lock (mLock)
                {
                    mTremor1.Enqueue(value);
                    if (mTremor1.Count > INTEGRATE_COUNT)
                        mTremor1.Dequeue();
                }
            }
        }

        public int Tremor2
        {
            get
            {
                lock (mLock)
                {
                    if (mTremor2.Count == 0)
                        return 0;
                    return (int)mTremor2.Average();
                }
            }
            private set
            {
                lock (mLock)
                {
                    mTremor2.Enqueue(value);
                    if (mTremor2.Count > INTEGRATE_COUNT)
                        mTremor2.Dequeue();
                }
            }
        }

        public SimpleVM()
        {
            mDevice.Stop();
            mDevice.AddListener(HandCallBack);
            mDevice.DemoMode = false;


            mTimer = new DispatcherTimer();
            mTimer.Tick += (sender, args) =>
            {
                NotifyPropertyChanged(m => m.Const1);
                NotifyPropertyChanged(m => m.Const2);
                NotifyPropertyChanged(m => m.Const1);
                NotifyPropertyChanged(m => m.Tremor1);
                NotifyPropertyChanged(m => m.Tremor2);
                NotifyPropertyChanged(m => m.State);
            };
            mTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / 25);
            mTimer.Start();
        }

        private void HandCallBack(ushort id, Hand hand1, Hand hand2)
        {
            Const1 = (int)hand1.Const.Average(s => s);
            Const2 = (int)hand2.Const.Average(s => s);
            Tremor1 = (int)hand1.Tremor.Average(s => s);
            Tremor2 = (int)hand2.Tremor.Average(s => s);
        }
    }
}