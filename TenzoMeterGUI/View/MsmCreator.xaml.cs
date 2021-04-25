using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using tEngine.DataModel;
using tEngine.Helpers;
using tEngine.MVVM;
using tEngine.Recorder;
using tEngine.TMeter;
using tEngine.TMeter.DataModel;
using MessageBox = System.Windows.MessageBox;

namespace TenzoMeterGUI.View
{
    /// <summary>
    /// Interaction logic for MsmCreator.xaml
    /// </summary>
    public partial class MsmCreator : Window
    {
        private MsmCreatorVM mDataContext;
        public bool? NotDialogButResult { get; set; }

        public Measurement Result => mDataContext == null ? null : mDataContext.CurrentMsm;

        public MsmCreator()
        {
            InitializeComponent();
            WindowManager.UpdateWindowPos(GetType().Name, this);
            mDataContext = new MsmCreatorVM() { Parent = this };
            DataContext = mDataContext;
        }

        public void PostSave() => mDataContext.PostSave();

        public void PostScript() => mDataContext.PostScript();

        public void SetMsm(Measurement msm) => mDataContext.SetMsm(msm);

        internal void PlotSelectedTab() => TabControl.SelectedIndex = 1;

        private void Window_OnClosing(object sender, CancelEventArgs e)
        {
            if (mDataContext != null)
            {
                try
                {
                    mDataContext.TimerProgress.Stop();
                    //mDataContext.PreClosed();
                    DialogResult = mDataContext.DialogResult;
                }
                catch (Exception )
                {
                    NotDialogButResult = mDataContext.DialogResult;
                }
            }
            WindowManager.SaveWindowPos(GetType().Name, this);
        }
    }

    public class MsmCreatorVM : Observed<MsmCreatorVM>
    {
        private bool mDoPostSave;
        private bool mDoPostScript;
        private bool mIsMsmRun;
        private string mSavePath;
        private string mScriptPath;
        private bool mSelectionEnable;
        private DispatcherTimer mTimerProgress;

        public int BeginPoint
        {
            get => CurrentMsm.Data.BeginPoint;
            set => CurrentMsm.Data.BeginPoint = value;
        }

        public Command CMDAcceptMsm { get; private set; }
        public Command CMDBrowse { get; private set; }
        public Command CMDCancelMsm { get; private set; }
        public Command CMDStartMsm { get; private set; }
        public Measurement CurrentMsm { get; private set; }

        public MsmCreatorVM()
        {
            CMDBrowse = new Command(CMDBrowse_Func);
            CMDAcceptMsm = new Command(CMDAcceptMsm_Func);
            CMDCancelMsm = new Command(CMDCancelMsm_Func);
            CMDStartMsm = new Command(CMDStartMsm_Func);

            DoPostSave = AppSettings.GetValue("DoPostSave", false);
            SavePath = AppSettings.GetValue("SavePath", "");
            DoPostScript = AppSettings.GetValue("DoPostScript", false);
            ScriptPath = AppSettings.GetValue("ScriptPath", "");

            CurrentMsm = new Measurement();

            SelectionEnable = true;

            mTimerProgress = new DispatcherTimer();
            mTimerProgress.Tick += TimerProgressOnTick;
            mTimerProgress.Interval = new TimeSpan(0, 0, 0, 0, 100);
            mTimerProgress.Stop();
            CurrentTime = "00:00.00";
            Progress = 20;
        }

        /// <summary>
        /// Время для вывода на прогрессбар
        /// </summary>
        public string CurrentTime { get; set; }

        public bool DoPostSave
        {
            get => mDoPostSave;
            set
            {
                mDoPostSave = value;
                NotifyPropertyChanged(m => m.DoPostSave);
            }
        }

        public bool DoPostScript
        {
            get => mDoPostScript;
            set
            {
                mDoPostScript = value;
                NotifyPropertyChanged(m => m.DoPostScript);
            }
        }

        public int EndPoint
        {
            get => CurrentMsm.Data.EndPoint;
            set => CurrentMsm.Data.EndPoint = value;
        }

        public string HandsData { get; set; }

        public bool IsMsmRun
        {
            get => mIsMsmRun;
            set
            {
                mIsMsmRun = value;
                NotifyPropertyChanged(m => m.IsMsmRun);
            }
        }

        public bool IsPauseBeforeStart { get; set; }

        public int Maximum => CurrentMsm.Data.CountBase;

        public double Progress { get; set; }

        public string SavePath
        {
            get => mSavePath;
            set
            {
                mSavePath = value;
                NotifyPropertyChanged(m => m.SavePath);
            }
        }

        public string ScriptPath
        {
            get => mScriptPath;
            set
            {
                mScriptPath = value;
                NotifyPropertyChanged(m => m.ScriptPath);
            }
        }

        public bool SelectionEnable
        {
            get => mSelectionEnable && CurrentMsm.Data.HasSomeData;
            set
            {
                mSelectionEnable = value;
                NotifyPropertyChanged(m => m.SelectionEnable);
            }
        }

        public DispatcherTimer TimerProgress => mTimerProgress;

        public void PostSave()
        {
            if (DoPostSave)
            {
                try
                {
                    CurrentMsm.Msm2CSV(SavePath);
                }
                catch(Exception ex) 
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public void PostScript()
        {
            if (DoPostScript)
            {
                try
                {
                    Process.Start(ScriptPath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public void SetMsm(Measurement msm)
        {
            CurrentMsm = new Measurement(msm);
            NotifyPropertyChanged(m => m.CurrentMsm);
        }

        private async void CMDAcceptMsm_Func()
        {
            if (CurrentMsm.Title.IsNullOrEmpty())
            {
                System.Windows.Forms.MessageBox.Show(@"Введите название измерения", @"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (CurrentMsm.Data.HasSomeData == false)
            {
                if (System.Windows.Forms.MessageBox.Show(@"Измерение не проведено, закрыть окно?", @"Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                    EndDialog(true);
                return;
            }
            if (CurrentMsm.Data.HasBaseData == false)
            {
                await Task.Factory.StartNew(() =>
                {
                    bool exit = false;
                    CurrentMsm.Data.BaseAnalys(null, (data, b) => { exit = true; });
                    MeasurementData.StartCalc();
                    while (exit == false)
                    {
                        Thread.Sleep(10);
                    }
                });
                EndDialog(true);
            }
            else
            {
                EndDialog(true);
            }
        }

        private void CMDBrowse_Func(object param)
        {
            if (param.Equals("save"))
            {

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "*.csv|*.csv";
                    if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        SavePath = sfd.FileName;
                    }
                }
            }
            else if (param.Equals("script"))
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "(Все файлы) *.*|*.*";
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        ScriptPath = ofd.FileName;
                    }
                }
            }

        }

        private void CMDCancelMsm_Func()
        {
            PreClosed();
            StopRecord();
            CurrentMsm = null;
            EndDialog(false);
        }

        private void CMDStartMsm_Func(object param)
        {
            if (param as bool? == true)
            {
                StartRecord();
            }
            else if (param as bool? == false)
            {
                StopRecord();
            }
        }

        private void HandCallBack(ushort id, Hand hand1, Hand hand2)
        {
            HandsData = string.Format("{0:F2} - {1:F2}", hand1.Const.Average(s => s), hand2.Const.Average(s => s));
            NotifyPropertyChanged(m => m.HandsData);
            CurrentMsm.AddData(hand1, hand2);
        }

        private void StartRecord()
        {
            SelectionEnable = false;
            CurrentMsm.Data.Clear();
            CurrentTime = "00:00.00";
            Progress = 0;
            TimeSpan timeOffset = new TimeSpan(0, 0, 0, 0);
            if (IsPauseBeforeStart)
            {
                timeOffset = new TimeSpan(0, 0, 0, 10);
            }
            CurrentMsm.CreateTime = (DateTime.Now + timeOffset);
            mTimerProgress.Start();

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(timeOffset);
                Device device = Device.GetDevice(Constants.DEVICE_ID);
                device.Start();
                device.AddListener(HandCallBack);
            });

            IsMsmRun = true;
        }

        private void StopRecord()
        {
            SelectionEnable = true;
            IsMsmRun = false;
            mTimerProgress.Stop();
            Device device = Device.GetDevice(Constants.DEVICE_ID);
            device.RemoveListener(HandCallBack);
            //device.Stop();
            Progress = 100;
            CurrentTime = new TimeSpan(0, 0, 0, 0, (int)(CurrentMsm.MsmTime * 1000.0)).ToString(@"mm\:ss\.ff");

            UpdateAllProperties();

            NotifyPropertyChanged(m => m.CurrentMsm.Data);
            NotifyPropertyChanged(m => m.BeginPoint);
            NotifyPropertyChanged(m => m.EndPoint);
            NotifyPropertyChanged(m => m.Maximum);

            Debug.Assert(Parent is MsmCreator);
            if (SelectionEnable)
            {
                ((MsmCreator)Parent).PlotSelectedTab();
            }
        }

        private void TimerProgressOnTick(object sender, EventArgs e)
        {
            double maxTime = CurrentMsm.MsmTime;
            TimeSpan time = DateTime.Now - CurrentMsm.CreateTime;
            double seconds = time.TotalMilliseconds / 1000.0;
            if (seconds > maxTime)
            {
                StopRecord();

                CurrentMsm.Data.BaseAnalys(null, (data, b) =>
                {
                    // считает быстро, но неплохо бы добавить бублик
                    Parent.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new Action(() => { MessageBox.Show("Измерение завершено!"); }));
                });
                MeasurementData.StartCalc();
                return;
            }
            CurrentTime = time.ToString(@"mm\:ss\.ff");
            Progress = (100.0 / maxTime) * seconds;
            NotifyPropertyChanged(m => m.Progress);
            NotifyPropertyChanged(m => m.CurrentTime);
        }

        public void PreClosed()
        {
            AppSettings.SetValue("DoPostSave", DoPostSave);
            AppSettings.SetValue("SavePath", SavePath);
            AppSettings.SetValue("DoPostScript", DoPostScript);
            AppSettings.SetValue("ScriptPath", ScriptPath);
            AppSettings.SetValue("DataTime", CurrentMsm.MsmTime);
        }
    }
}