using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using tEngine.DataModel;
using tEngine.Helpers;
using tEngine.Markers;
using tEngine.MVVM;
using tEngine.Recorder;
using tEngine.TMeter.DataModel;
using TenzoMeterGUI.View;

namespace TMSingleMeasurement.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    // TODO проверка ввода на "только цифры"
    // Выбор диапазона для измерений
    public partial class MarkersControl : Window
    {
        private MainControlVM mDataContext;

        public Measurement Result
        {
            get { return mDataContext == null ? null : mDataContext.Msm; }
        }

        public MarkersControl()
        {
            InitializeComponent();

            WindowManager.UpdateWindowPos(this.GetType().Name, this);
            mDataContext = new MainControlVM() { Parent = this };
            DataContext = mDataContext;
        }

        public void Init(Measurement msm = null)
        {
            mDataContext.SetMsm(msm);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (mDataContext != null)
            {
                mDataContext.PreClosed();
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

    public class MainControlVM : Observed<MainControlVM>
    {
        private const int INTEGRATE_COUNT = 25;
        private Device mDevice = Device.GetDevice(0);
        private bool mIsStart;
        private Queue<int> mLeft = new Queue<int>();
        private object mLock = new object();
        private Measurement mMsm;
        private int mMsmTime;
        private Queue<int> mRight = new Queue<int>();
        private DispatcherTimer mTimerProgress;

        /// <summary>
        /// Закрыть окно
        /// </summary>
        public Action Close { get; set; }

        public Command CMDAccept { get; private set; }
        public Command CMDCancel { get; private set; }
        public Command CMDMarkersSetShow { get; private set; }
        public Command CMDMarkersWindowShow { get; private set; }
        public Command CMDStart { get; private set; }
        public Command CMDStartMeasurement { get; private set; }
        public Command CMDStop { get; private set; }
        public Command CMDStopMeasurement { get; private set; }
        public string CurrentTime { get; set; }

        public bool IsStart
        {
            get { return mIsStart; }
            set
            {
                mIsStart = value;
                NotifyPropertyChanged(m => m.IsStart);
            }
        }

        public int Left
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
                NotifyPropertyChanged(m => m.Left);
            }
        }

        public Measurement Msm
        {
            get { return mMsm; }
            private set
            {
                mMsm = value;
                MsmTime = (int)Msm.MsmTime;
            }
        }

        public int MsmCount { get; set; }

        public int MsmTime
        {
            get { return mMsmTime; }
            set
            {
                mMsmTime = value;
                NotifyPropertyChanged(m => m.MsmTime);
            }
        }

        public double Progress { get; set; }

        public int Right
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
                NotifyPropertyChanged(m => m.Right);
            }
        }

        public double Speed { get; set; }

        public MainControlVM()
        {
            CMDMarkersSetShow = new Command(MarkersSetShow);
            CMDMarkersWindowShow = new Command(MarkersWindowShow);
            CMDStartMeasurement = new Command(StartMeasurement);
            CMDStopMeasurement = new Command(StopMeasurement);
            CMDStart = new Command(Start);
            CMDStop = new Command(Stop);
            CMDCancel = new Command(Cancel);
            CMDAccept = new Command(Accept);

            mDevice.DemoMode = false;
            mDevice.AddListener(HandCallBack);
        }

        /// <summary>
        /// Сохранение настроек перед выходом и т.п.
        /// </summary>
        public void PreClosed()
        {
            Markers.CloseWindow();
            mDevice.RemoveListener(HandCallBack);
            Device.AbortAll();
        }

        public void SetMsm(Measurement msm)
        {
            Msm = new Measurement(msm ?? new Measurement());
        }

        private void Accept()
        {
            // принимаются только законченные измерения
            EndDialog(dialogResult: !IsStart);
        }

        private void Cancel()
        {
            EndDialog(dialogResult: false);
        }

        /// <summary>
        /// запись в измерение
        /// </summary>
        /// <param name="requestID"></param>
        /// <param name="hand1"></param>
        /// <param name="hand2"></param>
        private void HandCallBack(ushort requestID, Hand hand1, Hand hand2)
        {
            Left = (int)hand1.Const.Average(s => s);
            Right = (int)hand2.Const.Average(s => s);
            if (IsStart)
                Msm.AddData(hand1, hand2);
        }

        private void MarkersSetShow()
        {
            MarkersSet ms = new MarkersSet();
            if (ms.ShowDialog() == true)
            {
                if (Markers.IsWindow)
                    Markers.GetWindow().UpdateSettings();
            }
        }

        private void MarkersWindowShow()
        {
            Markers wnd = Markers.GetWindow();
            wnd.Show();
        }

        private void Start()
        {
            if (!IsStart)
            {
                Msm.Clear();
                Msm.CreateTime = DateTime.Now;
                IsStart = true;
                StartMeasurement();
                mTimerProgress = new DispatcherTimer();
                mTimerProgress.Tick += TimerProgressOnTick;
                mTimerProgress.Interval = new TimeSpan(0, 0, 0, 0, 1000 / 25);
                mTimerProgress.Start();
            }
        }

        private void StartMeasurement()
        {
            mDevice.Start();
        }

        private void Stop()
        {
            if (IsStart)
            {
                IsStart = false;
                Msm.MsmTime = (DateTime.Now - Msm.CreateTime).TotalMilliseconds / 1000.0;
                mTimerProgress.Stop();
            }
        }

        private void StopMeasurement()
        {
            mDevice.Stop();
        }

        private void TimerProgressOnTick(object sender, EventArgs e)
        {
            TimeSpan time = DateTime.Now - Msm.CreateTime;
            double seconds = time.TotalMilliseconds / 1000.0;
            if (seconds >= MsmTime)
            {
                Stop();
                //time = new TimeSpan( 0, 0, (int)seconds );
                time = new TimeSpan(0, 0, MsmTime);
            }

            CurrentTime = time.ToString(@"mm\:ss\.ff");

            Progress = (100.0 / MsmTime) * seconds;

            MsmCount = Msm.DataLength();
            Speed = MsmCount / seconds;

            NotifyPropertyChanged(m => m.Progress);
            NotifyPropertyChanged(m => m.MsmCount);
            NotifyPropertyChanged(m => m.Speed);


            NotifyPropertyChanged(m => m.CurrentTime);
        }
    }
}