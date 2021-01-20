﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using tEngine.DataModel;
using tEngine.Helpers;
using tEngine.MVVM;
using tEngine.Recorder;
using tEngine.TMeter;
using tEngine.TMeter.DataModel;
using MessageBox = System.Windows.MessageBox;

namespace TenzoMeterGUI.View {
    /// <summary>
    /// Interaction logic for MsmCreator.xaml
    /// </summary>
    public partial class MsmCreator : Window {
        private MsmCreatorVM mDataContext;
        public bool? NotDialogButResult { get; set; }

        public Msm Result {
            get { return mDataContext == null ? null : mDataContext.CurrentMsm; }
        }

        public MsmCreator() {
            InitializeComponent();
            WindowManager.UpdateWindowPos( this.GetType().Name, this );
            mDataContext = new MsmCreatorVM() { Parent = this };
            DataContext = mDataContext;
        }

        public void PostSave() {
            mDataContext.PostSave();
        }

        public void PostScript() {
            mDataContext.PostScript();
        }

        public void SetMsm( Msm msm ) {
            mDataContext.SetMsm( msm );
        }

        internal void PlotSelectedTab() {
            TabControl.SelectedIndex = 1;
        }

        private void Window_OnClosing( object sender, CancelEventArgs e ) {
            if ( mDataContext != null ) {
                try {
                    mDataContext.TimerProgress.Stop();
                    mDataContext.PreClosed();
                    DialogResult = mDataContext.DialogResult;
                } catch ( Exception ex ) {
                    //Debug.Assert( false, ex.Message );
                    NotDialogButResult = mDataContext.DialogResult;
                }
            }
            WindowManager.SaveWindowPos( this.GetType().Name, this );
        }
    }

    public class MsmCreatorVM : Observed<MsmCreatorVM> {
        private bool mDoPostSave;
        private bool mDoPostScript;
        private bool mIsMsmRun;
        private string mSavePath;
        private string mScriptPath;
        private bool mSelectionEnable;
        private DispatcherTimer mTimerProgress;

        public int BeginPoint {
            get { return CurrentMsm.Data.BeginPoint; }
            set { CurrentMsm.Data.BeginPoint = value; }
        }

        public Command CMDAcceptMsm { get; private set; }
        public Command CMDBrowse { get; private set; }
        public Command CMDCancelMsm { get; private set; }
        public Command CMDStartMsm { get; private set; }
        public Msm CurrentMsm { get; private set; }

        /// <summary>
        /// Время для вывода на прогрессбар
        /// </summary>
        public string CurrentTime { get; set; }

        public bool DoPostSave {
            get { return mDoPostSave; }
            set {
                mDoPostSave = value;
                NotifyPropertyChanged( m => m.DoPostSave );
            }
        }

        public bool DoPostScript {
            get { return mDoPostScript; }
            set {
                mDoPostScript = value;
                NotifyPropertyChanged( m => m.DoPostScript );
            }
        }

        public int EndPoint {
            get { return CurrentMsm.Data.EndPoint; }
            set { CurrentMsm.Data.EndPoint = value; }
        }

        public string HandsData { get; set; }

        public bool IsMsmRun {
            get { return mIsMsmRun; }
            set {
                mIsMsmRun = value;
                NotifyPropertyChanged( m => m.IsMsmRun );
            }
        }

        public bool IsPauseBeforeStart { get; set; }

        public int Maximum {
            get { return CurrentMsm.Data.CountBase; }
        }

        public double Progress { get; set; }

        public string SavePath {
            get { return mSavePath; }
            set {
                mSavePath = value;
                NotifyPropertyChanged( m => m.SavePath );
            }
        }

        public string ScriptPath {
            get { return mScriptPath; }
            set {
                mScriptPath = value;
                NotifyPropertyChanged( m => m.ScriptPath );
            }
        }

        public bool SelectionEnable {
            get { return mSelectionEnable && CurrentMsm.Data.IsSomeData; }
            set {
                mSelectionEnable = value;
                NotifyPropertyChanged( m => m.SelectionEnable );
            }
        }

        public DispatcherTimer TimerProgress {
            get { return mTimerProgress; }
        }

        public MsmCreatorVM() {
            CMDBrowse = new Command( CMDBrowse_Func );
            CMDAcceptMsm = new Command( CMDAcceptMsm_Func );
            CMDCancelMsm = new Command( CMDCancelMsm_Func );
            CMDStartMsm = new Command( CMDStartMsm_Func );

            
            DoPostSave = AppSettings.GetValue( "DoPostSave", false );
            SavePath = AppSettings.GetValue( "SavePath", "" );
            DoPostScript = AppSettings.GetValue( "DoPostScript", false );
            ScriptPath = AppSettings.GetValue( "ScriptPath", "" );

            CurrentMsm = new Msm();

            SelectionEnable = true;

            mTimerProgress = new DispatcherTimer();
            mTimerProgress.Tick += TimerProgressOnTick;
            mTimerProgress.Interval = new TimeSpan( 0, 0, 0, 0, 1000 / 10 ); //скорость обновления
            mTimerProgress.Stop();
            CurrentTime = "00:00.00";
            Progress = 20;
        }

        public void PostSave() {
            if ( DoPostSave ) {
                try {
                    CurrentMsm.Msm2CSV( SavePath );
                } catch { }
            }
        }

        public void PostScript() {
            if ( DoPostScript ) {
                try {
                    Process.Start( ScriptPath );
                } catch { }
            }
        }

        public void SetMsm( Msm msm ) {
            CurrentMsm = new Msm( msm );
            //Cloner.CopyAllFields( CurrentMsm, msm );
            //Cloner.CopyAllProperties( CurrentMsm, msm );
            //CurrentMsm.SetData( msm.Data );
            NotifyPropertyChanged( m => m.CurrentMsm );
        }

        private async void CMDAcceptMsm_Func() {
            if ( CurrentMsm.Title.IsNullOrEmpty() ) {
                System.Windows.Forms.MessageBox.Show( @"Введите название измерения", @"Ошибка", MessageBoxButtons.OK,
                    MessageBoxIcon.Error );
                return;
            }
            if ( CurrentMsm.Data.IsSomeData == false ) {
                if ( System.Windows.Forms.MessageBox.Show( @"Измерение не проведено, закрыть окно?", @"Предупреждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning ) == System.Windows.Forms.DialogResult.Yes ) {
                    EndDialog( true );
                    return;
                } else {
                    return;
                }
            }
            if ( CurrentMsm.Data.IsBaseData == false ) {
                await Task.Factory.StartNew( () => {
                    var exit = false;
                    CurrentMsm.Data.BaseAnalys( null, ( data, b ) => { exit = true; } );
                    TData.StartCalc();
                    while ( exit == false ) {
                        Thread.Sleep( 10 );
                    }
                } );
                EndDialog( true );
            } else {
                EndDialog( true );
            }
        }

        private void CMDBrowse_Func( object param ) {
            if ( param.Equals( "save" ) ) {

                using ( var sfd = new SaveFileDialog() ) {
                    sfd.Filter = "*.csv|*.csv";
                    if ( sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK ) {
                        SavePath = sfd.FileName;
                    }
                }
            } else if ( param.Equals( "script" ) ) {
                using ( var ofd = new OpenFileDialog() ) {
                    ofd.Filter = "(Все файлы) *.*|*.*";
                    if ( ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK ) {
                        ScriptPath = ofd.FileName;
                    }
                }
            }

        }

        private void CMDCancelMsm_Func() {
            StopRecord();
            CurrentMsm = null;
            EndDialog( false );
        }

        private void CMDStartMsm_Func( object param ) {
            if ( param as bool? == true ) {
                StartRecord();
            } else if ( param as bool? == false ) {
                StopRecord();
            }
        }

        private void HandCallBack( ushort id, Hand hand1, Hand hand2 ) {
            HandsData = string.Format( "{0:F2} - {1:F2}", hand1.Const.Average( s => s ), hand2.Const.Average( s => s ) );
            NotifyPropertyChanged( m => m.HandsData );
            CurrentMsm.AddData( hand1, hand2 );
        }

        private void StartRecord() {
            SelectionEnable = false;
            CurrentMsm.Data.Clear();
            CurrentTime = "00:00.00";
            Progress = 0;
            var timeOffset = new TimeSpan( 0, 0, 0, 0 );
            if ( IsPauseBeforeStart ) {
                timeOffset = new TimeSpan( 0, 0, 0, 10 );
            }
            CurrentMsm.CreateTime = (DateTime.Now + timeOffset);
            mTimerProgress.Start();

            Task.Factory.StartNew( () => {
                Thread.Sleep( timeOffset );
                var device = Device.GetDevice( Constants.DEVICE_ID );
                device.Start();
                device.AddListener( HandCallBack );
            } );

            IsMsmRun = true;
        }

        private void StopRecord() {
            SelectionEnable = true;
            IsMsmRun = false;
            mTimerProgress.Stop();
            var device = Device.GetDevice( Constants.DEVICE_ID );
            device.RemoveListener( HandCallBack );
            //device.Stop();
            Progress = 100;
            CurrentTime = new TimeSpan( 0, 0, 0, 0, (int)(CurrentMsm.MsmTime * 1000.0) ).ToString( @"mm\:ss\.ff" );

            UpdateAllProperties();

            NotifyPropertyChanged( m => m.CurrentMsm.Data );
            NotifyPropertyChanged( m => m.BeginPoint );
            NotifyPropertyChanged( m => m.EndPoint );
            NotifyPropertyChanged( m => m.Maximum );

            Debug.Assert( Parent is MsmCreator );
            if ( SelectionEnable ) {
                ((MsmCreator)Parent).PlotSelectedTab();
            }
        }

        private void TimerProgressOnTick( object sender, EventArgs e ) {
            double maxTime = CurrentMsm.MsmTime;
            var time = DateTime.Now - CurrentMsm.CreateTime;
            var seconds = time.TotalMilliseconds / 1000.0;
            if ( seconds > maxTime ) {
                StopRecord();

                CurrentMsm.Data.BaseAnalys( null, ( data, b ) => {
                    // считает быстро, но неплохо бы добавить бублик
                    Parent.Dispatcher.BeginInvoke( DispatcherPriority.Normal,
                        new Action( () => { MessageBox.Show( "Измерение завершено!" ); } ) );
                } );
                TData.StartCalc();
                return;
            }
            CurrentTime = time.ToString( @"mm\:ss\.ff" );
            Progress = (100.0 / maxTime) * seconds;
            NotifyPropertyChanged( m => m.Progress );
            NotifyPropertyChanged( m => m.CurrentTime );
        }

        public void PreClosed() {
            AppSettings.SetValue( "DoPostSave", DoPostSave );
            AppSettings.SetValue( "SavePath", SavePath );
            AppSettings.SetValue( "DoPostScript", DoPostScript );
            AppSettings.SetValue( "ScriptPath", ScriptPath );

            
            AppSettings.SetValue( "DataTime", CurrentMsm.MsmTime );
        }
    }
}