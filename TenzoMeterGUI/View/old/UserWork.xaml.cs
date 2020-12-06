using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using OxyPlot;
using tEngine.Helpers;
using tEngine.MVVM;
using tEngine.TMeter;
using tEngine.TMeter.DataModel;

namespace TenzoMeterGUI.View.old {
    /// <summary>
    /// Interaction logic for UserWork.xaml
    /// </summary>
    public partial class UserWork : Window {
        private UserWorkVM mDataContext;

        public User Result {
            get { return mDataContext == null ? null : mDataContext.GetUser(); }
        }

        public UserWork() {
            InitializeComponent();
            WindowManager.UpdateWindowPos( this.GetType().Name, this );
            mDataContext = new UserWorkVM() {Parent = this};
            DataContext = mDataContext;
        }

        public void Init( User user ) {
            mDataContext.SetUser( user );
        }

        private void FrameworkElement_OnSizeChanged( object sender, SizeChangedEventArgs e ) {
            LeftList.MaxWidth = this.Width*2/3;
        }

        private void Window_OnClosing( object sender, CancelEventArgs e ) {
            mDataContext.PreClosed();
            mDataContext.LeftListW = LeftList.ActualWidth;

            WindowManager.SaveWindowPos( this.GetType().Name, this );
        }
    }

    public class UserWorkVM : Observed<UserWorkVM> {
        private Msm mSelectedMsm;
        private User mUser;

        public int Age {
            get {
                var age = DateTime.Today.Year - mUser.BirthDate.Year;
                if( mUser.BirthDate > DateTime.Now.AddYears( -age ) ) age--;
                return age;
            }
        }

        public DateTime BirthDay {
            get { return mUser.BirthDate; }
            set {
                mUser.BirthDate = value;
                NotifyPropertyChanged( m => BirthDay );
            }
        }

        public Command CMDExitApplication { get; private set; }
        public Command CMDNewMsm { get; private set; }
        public Command CMDNewUser { get; private set; }
        public Command CMDOpenUser { get; private set; }
        public Command CMDSaveAsUser { get; private set; }
        public Command CMDSaveUser { get; private set; }
        public Command CMDSelectMsm { get; private set; }
        public Command CMDSettings { get; private set; }
        public Command CMDUserInfoShow { get; private set; }

        public string Comment {
            get { return mUser.Comment; }
            set {
                mUser.Comment = value;
                NotifyPropertyChanged( m => m.Comment );
            }
        }

        public string FIO {
            get { return string.Format( "{0} {1} {2}", mUser.SName, mUser.Name, mUser.FName ); }
        }

        public string FName {
            get { return mUser.FName; }
            set {
                mUser.FName = value;
                NotifyPropertyChanged( m => m.FName );
                NotifyPropertyChanged( m => m.UShort );
            }
        }

        public IEnumerable<IndexedObject<Msm>> IndexedMsms {
            get {
                return mUser.GetMsms().ToList().Select( ( msm, i ) => new IndexedObject<Msm>( msm, i + 1 ) ).ToList();
            }
        }

        public bool IsMsmSelected {
            get { return (mSelectedMsm != null) || IsDesignMode; }
        }

        public double LeftListW {
            get { return AppSettings.GetValue( "UserWork_LeftListW", 200.0 ); }
            set { AppSettings.SetValue( "UserWork_LeftListW", value ); }
        }

        public MsmInfoWM MsmInfoDC { get; set; }

        public string MsmTitle {
            get { return mSelectedMsm.Title; }
            set {
                mSelectedMsm.Title = value;
                NotifyPropertyChanged( m => m.MsmTitle );
            }
        }

        public string Name {
            get { return mUser.Name; }
            set {
                mUser.Name = value;
                NotifyPropertyChanged( m => m.Name );
                NotifyPropertyChanged( m => m.UShort );
            }
        }

        public string SMsmTitle {
            get { return mSelectedMsm.Title; }
        }

        public string SName {
            get { return mUser.SName; }
            set {
                mUser.SName = value;
                NotifyPropertyChanged( m => m.SName );
                NotifyPropertyChanged( m => m.UShort );
            }
        }

        public string UShort {
            get { return mUser.UserShort(); }
        }

        public UserWorkVM() {
            CMDNewMsm = new Command( NewMsm );
            CMDSelectMsm = new Command( SelectMsm );
            CMDNewUser = new Command( NewUser );
            CMDOpenUser = new Command( OpenUser );
            CMDSaveUser = new Command( SaveUser );
            CMDSaveAsUser = new Command( SaveAsUser );
            CMDSettings = new Command( Settings );
            CMDExitApplication = new Command( ExitApplication );
            CMDUserInfoShow = new Command( UserInfoShow );
            if( IsDesignMode ) {
                //SetUser( User.GetTestUser() );
            } else {
                SetUser( new User() );
            }
        }

        public User GetUser() {
            return mUser;
        }

        public void PreClosed() {
        }

        public void SetUser( User user ) {
            mUser = new User( user );
            SelectMsm( mUser.GetMsm( 0 ) );
            // update all properties
            this.GetType()
                .GetProperties()
                .Where( info => info.CanRead )
                .ToList()
                .ForEach( info => { NotifyPropertyChanged( info.Name ); } );
        }

        private void ExitApplication() {
            if( Parent != null )
                Parent.Close();
        }

        private void NewMsm() {
            var nm = WindowManager.NewWindow<CreatorMsm_old>();
            var defTitle = string.Format( "Измерение №{0}", mUser.GetMsmCount() + 1 );
            nm.Init( defTitle );
            nm.ShowDialog();
            if( nm.Result != null ) {
                mUser.AddMsm( nm.Result );
                SelectMsm( nm.Result );
                NotifyPropertyChanged( m => m.IndexedMsms );
            }
        }

        private void NewUser() {
            var ui = new UserInfo();
            ui.EditMode = true;
            if( ui.ShowDialog() == true ) {
                if( ui.Result != null ) {
                    SetUser( ui.Result );
                }
            }
        }

        private void OpenUser() {
            var ofd = new OpenFileDialog();
            ofd.Filter = string.Format( "*{0}|*{0}", Constants.USER_EXT );
            if( ofd.ShowDialog() == true ) {
                User newUser;
                var result = User.Open( ofd.FileName, out newUser );
                if( result ) {
                    SetUser( newUser );
                } else {
                    // TODO Bad file select
                }
            }
        }

        private void SaveAsUser() {
            var sfd = new SaveFileDialog();
            sfd.Filter = string.Format( "*{0}|*{0}", Constants.USER_EXT );
            if( sfd.ShowDialog() == true ) {
                mUser.Save( sfd.FileName );
            }
        }

        private void SaveUser() {
            if( mUser != null )
                mUser.Save();
        }

        private void SelectMsm( object param ) {
            if( param is Msm )
                mSelectedMsm = param as Msm;
            else if( param is IndexedObject<Msm> )
                mSelectedMsm = (param as IndexedObject<Msm>).Value;

            MsmInfoDC = new MsmInfoWM();
            MsmInfoDC.SetMsm( mSelectedMsm );

            NotifyPropertyChanged( m => m.MsmInfoDC );
            NotifyPropertyChanged( m => m.SMsmTitle );
            NotifyPropertyChanged( m => m.IsMsmSelected );
        }

        private void Settings() {
            throw new NotImplementedException();
        }

        private void UserInfoShow() {
            var uInfo = new UserInfo();
            uInfo.SetUser( mUser );
            uInfo.EditMode = false;
            if( uInfo.ShowDialog() != null ) {
                if( uInfo.Result != null ) {
                    SetUser( uInfo.Result );
                }
            }
        }
    }

    public class MsmInfoWM : Observed<MsmInfoWM> {
        private bool mIsSelected = false;
        private Msm mMsm;
        private PlotModel mPMConst = null;
        private PlotModel mPMSpectrum = null;
        private PlotModel mPMTremor = null;
        public Command CMDBasedAnalys { get; private set; }
        public Command CMDPlotReset { get; private set; }
        public Command CMDSpectrumAnalys { get; private set; }
        public Command CMDStartMsm { get; private set; }

        public string Comment {
            get { return mMsm.Comment; }
            set {
                mMsm.Comment = value;
                NotifyPropertyChanged( m => m.Comment );
            }
        }

        public DateTime CreateTime {
            get { return mMsm.CreateTime; }
        }

        public bool IsSpectrumReady {
            get { return mMsm.Data.IsSpectrum || IsDesignMode; }
        }

        public double LeftMax {
            get {
                var max = mMsm.Data.Left.Constant.Max;
                if( double.IsNaN( max ) )
                    max = 0;
                return max;
            }
        }

        public double LeftMean {
            get {
                var mean = mMsm.Data.Left.Constant.Mean;
                if( double.IsNaN( mean ) )
                    mean = 0;
                return mean;
            }
        }

        public double LeftMin {
            get {
                var min = mMsm.Data.Left.Constant.Min;
                if( double.IsNaN( min ) )
                    min = 0;
                return min;
            }
        }

        public double LeftTremor {
            get {
                var min = mMsm.Data.Left.Tremor.Min;
                var max = mMsm.Data.Left.Tremor.Max;
                if( double.IsNaN( max ) || double.IsNaN( min ) )
                    max = 0;
                return (max - min);
            }
        }

        public double LeftTremorMax {
            get {
                var max = mMsm.Data.Left.Tremor.Max;
                if( double.IsNaN( max ) )
                    max = 0;
                return max;
            }
        }

        public double LeftTremorMin {
            get {
                var min = mMsm.Data.Left.Tremor.Min;
                if( double.IsNaN( min ) )
                    min = 0;
                return min;
            }
        }

        public double LeftTremorPercent {
            get {
                var force = Math.Abs( LeftMean );
                var tremor = LeftTremor;
                var percent = tremor/force;
                return percent;
            }
        }

        public double MsmTime {
            get { return mMsm.MsmTime; }
        }

        //public PlotModel PMConst {
        //    get {
        //        // TODO 
        //        if( mPMConst == null ) {
        //            mPMConst = new PlotModel();
        //            mPMConst.Title = "Постоянная состовляющая";
        //            mPMConst.CreateModel_1();
        //            //mPMConst.AddSelector( ( finish, min, max ) => { if( finish ) PMTremor.SetSelection( min, max ); } );
        //        }
        //        mPMConst.Series.Clear();
        //        mPMConst.Series.SetLine( mMsm.Data.GetConst( Hands.Left).GetPartExactly( 100 ), title: "Левая рука");
        //        mPMConst.Series.SetLine( mMsm.Data.GetConst( Hands.Right).GetPartExactly( 100 ), title: "Правая рука");

        //        var lMin = LeftMean - LeftMin;
        //        var rMin = RightMean - RightMax;
        //        var lMax = LeftMean + LeftMax;
        //        var rMax = RightMean + RightMax;
        //        if( lMin > 0 && rMin > 0 ) {
        //            mPMConst.Axes.ToList().ForEach( axis => {
        //                if( axis.Position == AxisPosition.Left || axis.Position == AxisPosition.Right )
        //                    axis.Minimum = 0;
        //            } );
        //        } else if( lMax < 0 && rMax < 0 ) {
        //            mPMConst.Axes.ToList().ForEach( axis => {
        //                if( axis.Position == AxisPosition.Left || axis.Position == AxisPosition.Right )
        //                    axis.Maximum = 0;
        //            } );
        //        }
        //        mPMConst.InvalidatePlot( true );
        //        return mPMConst;
        //    }
        //}

        //public PlotModel PMSpectrum {
        //    get {
        //        // TODO пустое series портит вызывает исключение при нажатии
        //        // либо ставить ноль, либо не ставить вообще
        //        if( mPMSpectrum == null ) {
        //            mPMSpectrum = new PlotModel();
        //            mPMSpectrum.Title = "Спектр тремора";
        //            mPMSpectrum.CreateModel_1();
        //        }
        //        mPMSpectrum.Series.Clear();
        //        mPMSpectrum.Series.SetLine( mMsm.Data.GetSpectrum( Hands.Left, length: 1000 ), title: "Левая рука" );
        //        mPMSpectrum.Series.SetLine( mMsm.Data.GetSpectrum( Hands.Right, length: 1000 ), title: "Правая рука");

        //        mPMSpectrum.InvalidatePlot( true );
        //        return mPMSpectrum;
        //    }
        //}

        //public PlotModel PMTremor {
        //    get {
        //        // TODO 
        //        if( mPMTremor == null ) {
        //            mPMTremor = new PlotModel();
        //            mPMTremor.Title = "Тремор";
        //            mPMTremor.CreateModel_1();
        //            //mPMTremor.AddSelector( ( finish, min, max ) => { if( finish ) PMConst.SetSelection( min, max ); } );
        //        }
        //        mPMTremor.Series.Clear();
        //        mPMTremor.Series.SetLine( mMsm.Data.GetTremor( Hands.Left, length: 100 ), title: "Левая рука");
        //        mPMTremor.Series.SetLine( mMsm.Data.GetTremor( Hands.Right, length: 100 ), title: "Правая рука");
        //        mPMTremor.InvalidatePlot( true );
        //        return mPMTremor;
        //    }
        //}

        public bool ReadyToShow {
            get { return mMsm.Data.IsBaseData || IsDesignMode; }
        }

        public double RightMax {
            get {
                var max = mMsm.Data.Right.Constant.Max;
                if( double.IsNaN( max ) )
                    max = 0;
                return max;
            }
        }

        public double RightMean {
            get {
                var mean = mMsm.Data.Right.Constant.Mean;
                if( double.IsNaN( mean ) )
                    mean = 0;
                return mean;
            }
        }

        public double RightMin {
            get {
                var min = mMsm.Data.Right.Constant.Min;
                if( double.IsNaN( min ) )
                    min = 0;
                return min;
            }
        }

        public double RightTremor {
            get {
                var min = mMsm.Data.Right.Tremor.Min;
                var max = mMsm.Data.Right.Tremor.Max;
                if( double.IsNaN( max ) || double.IsNaN( min ) )
                    max = 0;
                return (max - min);
            }
        }

        public double RightTremorMax {
            get {
                var max = mMsm.Data.Right.Tremor.Max;
                if( double.IsNaN( max ) )
                    max = 0;
                return max;
            }
        }

        public double RightTremorMin {
            get {
                var min = mMsm.Data.Right.Tremor.Min;
                if( double.IsNaN( min ) )
                    min = 0;
                return min;
            }
        }

        public double RightTremorPercent {
            get {
                var force = Math.Abs( RightMean );
                var tremor = RightTremor;
                var percent = tremor/force;
                return percent;
            }
        }

        public string Title {
            get { return mMsm.Title; }
        }

        public MsmInfoWM() {
            CMDBasedAnalys = new Command( BasedAnalys );
            CMDPlotReset = new Command( PlotReset );
            CMDSpectrumAnalys = new Command( SpectrumAnalys );
            CMDStartMsm = new Command( StartMsm );

            SetMsm();
            //if( IsDesignMode )
            //    SetMsm( Msm.GetTestMsm() );
        }

        public void SetMsm( Msm msm = null ) {
            mMsm = msm ?? new Msm();
            UpdateAllProperties();
        }

        private void BasedAnalys() {
            mMsm.Data.BaseAnalys( null, ( data, error ) => { UpdateAllProperties(); } );
        }

        private void PlotReset( object param ) {
            if( !(param is string) ) return;
            var commands = (param as string).Replace( " ", "" )
                .Split( new[] {"||"}, StringSplitOptions.RemoveEmptyEntries );
            if( commands.Any() ) {
                commands.ToList().ForEach( cmd => {
                    //if( cmd.Equals( "Const" ) ) {
                    //    PMConst.ResetAllAxes();
                    //    NotifyPropertyChanged( m => m.PMConst );
                    //} else if( cmd.Equals( "Tremor" ) ) {
                    //    PMTremor.ResetAllAxes();
                    //    NotifyPropertyChanged( m => m.PMTremor );
                    //} else if( cmd.Equals( "Spectrum" ) ) {
                    //    PMSpectrum.ResetAllAxes();
                    //    NotifyPropertyChanged( m => m.PMSpectrum );
                    //}
                } );
            }
        }

        private void SpectrumAnalys() {
            var wnd = new SpectrumProgress();
            wnd.Show( new[] {mMsm.Data} );
            UpdateAllProperties();
        }

        private void StartMsm() {
            var wnd = new TMSingleMeasurement.View.MarkersControl();
            wnd.Init( mMsm );
            if( wnd.ShowDialog() == true ) {
                SetMsm( wnd.Result );
                wnd.Result.Data.BaseAnalys( null, ( data, b ) => UpdateAllProperties() );
            }
        }

        private void UpdateAllProperties() {
            this.GetType()
                .GetProperties()
                .Where( info => info.CanRead )
                .ToList()
                .ForEach( info => { NotifyPropertyChanged( info.Name ); } );
        }
    }
}