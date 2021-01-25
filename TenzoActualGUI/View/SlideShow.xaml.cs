using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using tEngine.DataModel;
using tEngine.Helpers;
using tEngine.MVVM;
using tEngine.Recorder;
using tEngine.TActual.DataModel;

namespace TenzoActualGUI.View
{
    /// <summary>
    /// Interaction logic for SlideShow.xaml
    /// </summary>
    public partial class SlideShow : Window
    {
        private const int INTEGRATE_COUNT = 25;
        private static Device mDevice;
        private static List<SlideShow> mMWindow = new List<SlideShow>();
        private SlideShowVM mDataContext;
        private Queue<int> mLeft = new Queue<int>();
        private object mLock = new object();
        private Queue<int> mRight = new Queue<int>();
        private DispatcherTimer mTimerDraw;

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

        public static SlideShow MWindow
        {
            get
            {
                if (mMWindow.Any())
                    return mMWindow[0];
                return null;
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

        public SlideShow()
        {
            InitializeComponent();
            mDevice = Device.CreateDevice(SlideShowVM.DEVICE_ID);
            WindowManager.UpdateWindowPos(this.GetType().Name, this);

            mDataContext = new SlideShowVM() { Parent = this };
            DataContext = mDataContext;

            mTimerDraw = new DispatcherTimer();
            mTimerDraw.Tick += TimerDrawOnTick;
            mTimerDraw.Interval = new TimeSpan(0, 0, 0, 0, 1000 / 25);
            mTimerDraw.Start();

            mDevice.AddListener(HandCallBack);
            mDevice.DemoMode = AppSettings.GetValue("IsDebug", true);
            UpdateSettings();
        }

        public static void CloseAll()
        {
            mMWindow.ForEach(m => m.Close());
            mMWindow.Clear();
        }

        // можно создать только одно окно
        public static SlideShow GetWindow()
        {
            if (mMWindow.Count == 0)
            {
                SlideShow wnd = new SlideShow();
                mMWindow.Add(wnd);
            }
            return mMWindow[0];
        }
        public static bool IsWindowCreate()
        {
            return mMWindow.Count != 0;
        }

        public void StartShow(PlayList playList, Action<Guid> currentSlideCallBack, Action<bool> finalCallBack, DateTime? toStart = null)
        {
            mDataContext.PlayList = playList;
            mDataContext.StartShow(playList.SecondsToSlide, currentSlideCallBack, finalCallBack, toStart);
        }

        public void UpdateSettings()
        {
            MarkersArea.UpdateArea();
        }

        private void HandCallBack(ushort requestID, Hand hand1, Hand hand2)
        {
            LeftHand = (int)hand1.Const.Average(s => s);
            RightHand = (int)hand2.Const.Average(s => s);
            mDataContext.HandCallBack(requestID, hand1, hand2);
        }

        private void TimerDrawOnTick(object sender, EventArgs eventArgs)
        {
            MarkersArea.DrawPart(LeftHand, RightHand);
        }


        private void Window_OnClosing(object sender, CancelEventArgs e)
        {
            if (mDataContext != null)
            {
                mDataContext.CancelShow();
                try
                {
                    DialogResult = mDataContext.DialogResult;
                }
                catch
                {
                    /*если окно не диалог - вылетит исключение, ну и пусть*/
                }
            }
            mDevice.RemoveListener(HandCallBack);
            mDevice.Abort();
            WindowManager.SaveWindowPos(this.GetType().Name, this);
            mMWindow.Clear();
            mDataContext.PreClosed();
            if (PreClosed != null)
                PreClosed();
        }

        public Action PreClosed { get; set; }

        public void ShowCancel()
        {
            mDataContext.CancelShow();
        }
    }

    public class SlideShowVM : Observed<SlideShowVM>
    {
        internal const int DEVICE_ID = 11;
        private ushort mCurrentShortID;
        private Slide mCurrentSlide;
        private bool mIsNew = false;
        private object mLock = new object();
        private List<SlideID> mSlideList = new List<SlideID>();
        private DateTime mStartTime = DateTime.Now;
        private CancellationTokenSource mCancelTokenSource;

        public Slide CurrentSlide
        {
            get
            {
                lock (mLock)
                {
                    return mCurrentSlide;
                }
            }
            private set
            {
                lock (mLock)
                {
                    mCurrentSlide = value;
                }
            }
        }

        public PlayList PlayList { get; set; }
        public SlideShowVM() { }

        /// <summary>
        /// запись в измерение
        /// </summary>
        public void HandCallBack(ushort requestID, Hand hand1, Hand hand2)
        {
            if (CurrentSlide == null) return;
            if (requestID == mCurrentShortID)
            {
                // рисование картинки если пошли новые данные
                if (mIsNew)
                {
                    NotifyPropertyChanged(m => m.CurrentSlide);
                    mStartTime = DateTime.Now;
                    lock (mLock)
                    {
                        mIsNew = false;
                    }
                }
                CurrentSlide.Data.AddHands(hand1, hand2);
            }
            else
            {
                lock (mLock)
                {
                    mIsNew = true;
                }
            }
        }

        public void CancelShow()
        {
            if (mCancelTokenSource != null)
            {
                mCancelTokenSource.Cancel();
            }
            CurrentSlide = null;
            NotifyPropertyChanged(m => m.CurrentSlide);
        }

        public void StartShow(double secondToSlide, Action<Guid> currentSlideCallBack, Action<bool> finalCallBack, DateTime? toStart = null)
        {
            if (PlayList == null)
                return;
            if (toStart == null) toStart = DateTime.Now;
            PlayList.Slides.ToList().ForEach(slide => slide.Data.Clear());

            mSlideList = PlayList.Slides.Select((slide, i) => new SlideID(slide, i)).ToList();

            if (mCancelTokenSource != null)
            {
                mCancelTokenSource.Cancel();
                //mCancelTokenSource.Dispose();
            }
            mCancelTokenSource = new CancellationTokenSource();

            Action<bool> returnAction = new Action<bool>((error) =>
            {
                if (error)
                {
                }
                else
                {
                }
                if (finalCallBack != null)
                    finalCallBack(error);
            });
            Action<Guid> slideNotification = new Action<Guid>((id) =>
            {
                if (currentSlideCallBack != null)
                {
                    currentSlideCallBack(id);
                }
            });

            Task taska = Task.Factory.StartNew((param) =>
            {
                CancellationToken cancelTok = (CancellationToken)param;
                if (!cancelTok.IsCancellationRequested)
                {
                    while (DateTime.Now < toStart)
                    {
                        if (cancelTok.IsCancellationRequested)
                        {
                            returnAction(true);
                            return;
                        }
                        Thread.Sleep(10);
                    }

                    foreach (SlideID slide in mSlideList)
                    {
                        if (cancelTok.IsCancellationRequested)
                        {
                            returnAction(true);
                            return;
                        }
                        CurrentUpdate(slide.ID);
                        slideNotification(slide.ID);
                        // пока запрос считается новым и время не подошло
                        while (mIsNew == true || (DateTime.Now - mStartTime).TotalSeconds < secondToSlide)
                        {
                            Device.GetDevice(DEVICE_ID).NewRequest(slide.ShortID);
                            if (cancelTok.IsCancellationRequested)
                            {
                                returnAction(true);
                                return;
                            }
                            Thread.Sleep(10);
                        }
                        //смещаем id дабы не принимать новые значения
                        mCurrentShortID = (ushort)(mCurrentShortID - 1);
                        if (CurrentSlide != null)
                            CurrentSlide.Data.Time = (DateTime.Now - mStartTime).TotalSeconds;
                    }

                    //slideNotification( Guid.Empty );
                    returnAction(false);
                }
            }, mCancelTokenSource.Token, mCancelTokenSource.Token);

        }

        private void CurrentUpdate(Guid id)
        {
            SlideID slide = mSlideList.FirstOrDefault(slideID => slideID.ID.Equals(id));
            if (slide == null) return;
            CurrentSlide = slide.Slide;
            lock (mLock)
            {
                mCurrentShortID = slide.ShortID;
                mIsNew = true;
            }
        }

        private class SlideID
        {
            public Guid ID { get; private set; }
            public ushort ShortID { get; private set; }
            public Slide Slide { get; set; }

            public SlideID(Slide slide, int index)
            {
                ShortID = Convert.ToUInt16(index + 111);
                Slide = slide;
                ID = slide.Id;
            }
        }

        public void PreClosed()
        {
            Device.GetDevice(DEVICE_ID).Abort();
        }
    }
}