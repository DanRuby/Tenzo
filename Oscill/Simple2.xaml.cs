using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using tEngine.DataModel;
using tEngine.Helpers;
using tEngine.MVVM;
using tEngine.Recorder;

namespace Oscill
{
    /// <summary>
    /// Interaction logic for Simple2.xaml
    /// </summary>
    public partial class Simple2 : Window
    {
        private Simple2VM mDataContext;

        public Simple2()
        {
            AppSettings.Init(AppSettings.Project.Empty);
            InitializeComponent();
            WindowManager.UpdateWindowPos(this.GetType().Name, this);
            mDataContext = new Simple2VM() { Parent = this };
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
                catch (Exception)
                {
                    //Debug.Assert( false, ex.Message );
                }
            }
            WindowManager.SaveWindowPos(this.GetType().Name, this);
            AppSettings.Save();
        }
    }


    public class Simple2VM : Observed<Simple2VM>
    {
        private const int INTEGRATE_COUNT = 10;
        private short[] mAdcData = new short[4];
        private Queue<int> mConst1 = new Queue<int>();
        private Queue<int> mConst2 = new Queue<int>();
        private bool[] mDataReady = new bool[4];
        private Device mDevice = Device.CreateDevice(0);
        private bool mIsRun;
        private object mLock = new object();
        private string mLog;
        private string mOldState = "";
        private DispatcherTimer mTimer;
        private Queue<int> mTremor1 = new Queue<int>();
        private Queue<int> mTremor2 = new Queue<int>();
        private Device.WorkMode WorkMode = Device.WorkMode.Normal;
        public Command CMDModeChange { get; private set; }

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
                if (mIsRun)
                {
                    mDevice.Start();
                    mDevice.Counters.Clear();
                }
                else mDevice.Stop();
                NotifyPropertyChanged(m => m.IsRun);
            }
        }

        public string Log
        {
            get { return mLog; }
            set
            {
                mLog = value;
                NotifyPropertyChanged(m => m.Log);
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

        public Simple2VM()
        {
            CMDModeChange = new Command(CMDModeChange_Func);

            mDevice.Stop();
            mDevice.AdcTestCallBack = AdcTestCallBack;
            mDevice.AddListener(HandCallBack);
            mDevice.DemoMode = false;
            WorkMode = Device.WorkMode.Normal;
            mDevice.SetMode(Device.WorkMode.Normal);

            mTimer = new DispatcherTimer();
            mTimer.Tick += (sender, args) =>
            {
                ShowLog();
                if (mOldState != State)
                {
                    mDevice.SetMode(WorkMode);
                }
                mOldState = State;
                NotifyPropertyChanged(m => m.State);
            };
            mTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / 25);
            if (!IsDesignMode)
            {
                mTimer.Start();
                IsRun = false;
            }
        }

        private void AdcTestCallBack(bool[] dr, short[] adc)
        {
            if (dr == null || adc == null) return;
            if (dr.Length == 4 && adc.Length == 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (dr[i])
                    {
                        mAdcData[i] = adc[i];
                    }
                    mDataReady[i] = dr[i];
                }
                ShowLog();
            }
        }

        private void CMDModeChange_Func(object param)
        {
            if (param.Equals("normal"))
            {
                WorkMode = Device.WorkMode.Normal;
                mDevice.SetMode(WorkMode);
            }
            else if (param.Equals("adccheck"))
            {
                WorkMode = Device.WorkMode.AdcCheck;
                mDevice.SetMode(WorkMode);
            }
        }

        private void HandCallBack(ushort id, Hand hand1, Hand hand2)
        {
            Const1 = (int)hand1.Const.Average(s => s);
            Const2 = (int)hand2.Const.Average(s => s);
            Tremor1 = (int)hand1.Tremor.Average(s => s);
            Tremor2 = (int)hand2.Tremor.Average(s => s);
        }

        private void ShowLog()
        {
            StringBuilder str = new StringBuilder();
            if (WorkMode == Device.WorkMode.AdcCheck)
            {
                str.AppendLine(string.Format("Значение:\t{0}", string.Join("\t", mAdcData)));
                str.AppendLine(string.Format("Data Ready:\t{0}", string.Join("\t", mDataReady)));

                mDataReady = new[] { false, false, false, false };
            }
            else if (WorkMode == Device.WorkMode.Normal)
            {
                str.AppendLine(Environment.NewLine);

                int[] value = new int[] { Const1, Tremor1, Const2, Tremor2 };
                for (int i = 0; i < value.Length; i++)
                {
                    str.AppendLine("Канал " + i.ToString() + ":  " + value[i]);
                }
                str.AppendLine(Environment.NewLine);
                str.AppendLine("Статистика по пакетам:");
                str.AppendLine(string.Format("Всего................ {0}", mDevice.Counters.TotalPack));
                str.AppendLine(string.Format("Принято.............. {0}", mDevice.Counters.FullPack));
                str.AppendLine(string.Format("Отброшено............ {0}", mDevice.Counters.InvalidPack));
                str.AppendLine(string.Format("Пропущено............ {0}", mDevice.Counters.LostPack));
                str.AppendLine(string.Format("Повторений........... {0}", mDevice.Counters.RepeatPack));
                str.AppendLine(Environment.NewLine);
                str.AppendLine(string.Format("Пакетов/сек.......... {0:F2}", mDevice.Counters.PPS.GetPs()));
                str.AppendLine(string.Format("Принято/сек.......... {0:F2}", mDevice.Counters.ValidPPS.GetPs()));
                str.AppendLine(string.Format("Отсчетов АЦП/сек..... {0:F2}", mDevice.Counters.ValidPPS.GetPs() * 7));
            }
            Log = str.ToString();
        }
    }
}