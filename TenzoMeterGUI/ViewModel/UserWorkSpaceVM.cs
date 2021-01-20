using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using OxyPlot;
using OxyPlot.Wpf;
using tEngine.DataModel;
using tEngine.Helpers;
using tEngine.Markers;
using tEngine.MVVM;
using tEngine.PlotCreator;
using tEngine.TMeter.DataModel;
using tEngine.UControls;
using TenzoMeterGUI.View;
using MessageBox = System.Windows.MessageBox;

namespace TenzoMeterGUI.ViewModel {
    public class UserWorkSpaceVM : Observed<UserWorkSpaceVM> {
        private const int MINIMUM_POINTS = 100;
        private readonly Guid mNewMsmWindowID = Guid.NewGuid();
        private readonly Guid mResultWindowID = Guid.NewGuid();
        private ObservableCollection<DataToShow> mDataToShowList;
        private Dictionary<object, PlotViewEx> mGraphics = new Dictionary<object, PlotViewEx>();
        private bool mIsBusy = false;
        private Msm mSelectedMsm;
        private int mSelectedMsmIndex;
        private User mUser;
        public Command CMDCreateTextFile { get; private set; }
        public Command CMDEditSelectedMsm { get; private set; }
        public Command CMDExit { get; private set; }
        public Command CMDExportCSV { get; private set; }
        public Command CMDFocusChanged { get; private set; }
        public Command CMDNewMsm { get; private set; }
        public Command CMDOxyLoad { get; private set; }
        public Command CMDOxyPanelLoaded { get; private set; }
        public Command CMDOxyUnload { get; private set; }
        public Command CMDRemoveSelectedMsm { get; private set; }
        public Command CMDResetMsmList { get; private set; }
        public Command CMDResultShow { get; private set; }
        public Command CMDSaveOpenUser { get; private set; }
        public Command CMDShowMarkers { get; private set; }
        public Command CMDShowMarkersSettings { get; private set; }
        public Command CMDSpectrumCalc { get; private set; }
        public Command CMDUpdateOxy { get; private set; }

        public ObservableCollection<DataToShow> DataToShowList {
            get {
                if( mDataToShowList == null && IsMsm ) {
                    mDataToShowList = new ObservableCollection<DataToShow>() {
                        new DataToShow( "�����������", "����������� ����������� ������, ��� ����� �������",
                            SelectedMsm.Data.Left.Constant.Min, SelectedMsm.Data.Right.Constant.Min ),
                        new DataToShow( "�������", "������� ����������� ������",
                            SelectedMsm.Data.Left.Constant.Mean, SelectedMsm.Data.Right.Constant.Mean ),
                        new DataToShow( "������������", "������������ ����������� ������, ��� ����� �������",
                            SelectedMsm.Data.Left.Constant.Max, SelectedMsm.Data.Right.Constant.Max ),
                        new DataToShow( "������", "�������� ��������������� �������",
                            SelectedMsm.Data.GetTremorAmplitude( Hands.Left ),
                            SelectedMsm.Data.GetTremorAmplitude( Hands.Right ) ),
                        //new DataToShow( "������, %", "���� ������� ������������ �������� ������",
                        //    SelectedMsm.Data.GetTremorPercent( Hands.Left ),
                        //    SelectedMsm.Data.GetTremorPercent( Hands.Right ) ),
                    };
                }
                return mDataToShowList;
            }
        }

        // ��� �������
        public int IndexInList {
            get { return Msms.IndexOf( SelectedMsm ); }
        }

        public bool IsBusy {
            get { return mIsBusy; }
            set {
                mIsBusy = value;
                NotifyPropertyChanged( m => m.IsBusy );
            }
        }

        public new bool IsDesignMode {
            get { return true; }
        }

        public bool IsMarkersShow {
            get { return Markers.IsWindow; }
        }

        public bool IsMsm {
            get { return SelectedMsm != null; }
        }

        public bool IsNotSaveChanges {
            get { return User.IsNotSaveChanges; }
            set {
                User.IsNotSaveChanges = value;
                NotifyPropertyChanged( m => m.IsNotSaveChanges );
                NotifyPropertyChanged( m => m.WindowTitle );
            }
        }

        public ObservableCollection<Msm> Msms {
            get { return User.Msms; }
        }

        public bool SelectByIndex { get; set; }

        public Msm SelectedMsm {
            get { return mSelectedMsm; }
            set {
                mSelectedMsm = value;
                SelectByIndex = false;
                UpdateSelectedMsm();

                NotifyPropertyChanged( m => m.SelectedMsm );
                NotifyPropertyChanged( m => m.IsMsm );

                if( mSelectedMsm != null ) {
                    mDataToShowList = null;
                    NotifyPropertyChanged( m => m.DataToShowList );
                }

                NotifyPropertyChanged( m => m.IndexInList );
                CMDUpdateOxy_Func();
            }
        }

        public int SelectedMsmIndex {
            get { return mSelectedMsmIndex; }
            set {
                mSelectedMsmIndex = value;
                SelectByIndex = true;
                UpdateSelectedMsm();
            }
        }

        public User User {
            get { return mUser; }
            set {
                mUser = value;
                Debug.Assert( mUser != null );


                IsNotSaveChanges = false;
                SelectedMsm = Msms.FirstOrDefault();
                UpdateAllProperties();
            }
        }

        public string WindowTitle {
            get { return User.ToString( true ) + (IsNotSaveChanges ? "*" : ""); }
        }

        public UserWorkSpaceVM() {
            CMDExportCSV = new Command( CMDExportCSV_Func );
            CMDResultShow = new Command( CMDResultShow_Func );
            CMDOxyPanelLoaded = new Command( CMDOxyPanelLoaded_Func );
            CMDCreateTextFile = new Command( CMDCreateTextFile_Func );
            CMDNewMsm = new Command( CMDNewMsm_Func );
            CMDEditSelectedMsm = new Command( CMDEditSelectedMsm_Func );
            CMDResetMsmList = new Command( CMDResetMsmList_Func );
            CMDRemoveSelectedMsm = new Command( CMDRemoveSelectedMsm_Func );
            CMDSaveOpenUser = new Command( CMDSaveOpenUser_Func );
            CMDExit = new Command( CMDExit_Func );
            CMDShowMarkers = new Command( CMDShowMarkers_Func );
            CMDShowMarkersSettings = new Command( CMDShowMarkersSettings_Func );
            CMDFocusChanged = new Command( CMDFocusChanged_Func );
            CMDSpectrumCalc = new Command( CMDSpectrumCalc_Func );
            CMDOxyLoad = new Command( CMDOxyLoad_Func );
            CMDOxyUnload = new Command( CMDOxyUnload_Func );
            CMDUpdateOxy = new Command( CMDUpdateOxy_Func );

            if( IsDesignMode ) {
                User = User.GetTestUser();
            }
        }

        public void OpenMsm( Msm msm ) {
            if( msm == null ) return;
            ResetList( msm );
            IsNotSaveChanges = false;
            //throw new NotImplementedException();
        }

        public void PreClosed() {
            TData.CancelCalc();
        }

        public int R2Pixels( int percent ) {
            var data = SelectedMsm.Data;

            var max = data.Count;
            var min = (int) (max/10.0 > MINIMUM_POINTS ? MINIMUM_POINTS : max/10.0);
            var k = (max - min)/(100 - 1);
            var b = min - k*1.0;

            var result = percent*k + b;
            return (int) result;
        }

        private void CMDCreateTextFile_Func() {
            using( var sfd = new SaveFileDialog() ) {
                sfd.DefaultExt = ".txt";
                sfd.Filter = "*.txt|*.txt";
                sfd.InitialDirectory = Directory.GetCurrentDirectory();
                sfd.FileName = "text_adc";
                if( sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK ) {
                    var adc1 = SelectedMsm.Data.GetConst( Hands.Left ).Select( dp => dp.Y ).ToArray();
                    var adc2 = SelectedMsm.Data.GetTremor( Hands.Left ).Select( dp => dp.Y ).ToArray();
                    var adc3 = SelectedMsm.Data.GetConst( Hands.Right ).Select( dp => dp.Y ).ToArray();
                    var adc4 = SelectedMsm.Data.GetTremor( Hands.Right ).Select( dp => dp.Y ).ToArray();
                    var sb = new StringBuilder();
                    sb.Append( "���1\t���2\t���3\t���4" + Environment.NewLine );
                    for( int i = 0; i < adc1.Count(); i++ ) {
                        //sb.Append( string.Format( "{0}\t{1}\t{2}\t{3}", adc1[i],adc2[i],adc3[i],adc4[i]) + Environment.NewLine);
                        sb.Append( adc1[i] + "\t" + adc2[i] + "\t" + adc3[i] + "\t" + adc4[i] + Environment.NewLine );
                    }
                    using( var sw = new StreamWriter( sfd.FileName ) ) {
                        sw.Write( sb );
                    }
                    System.Windows.Forms.MessageBox.Show( "���� �������!" + Environment.NewLine + sfd.FileName );
                }
            }
        }

        private void CMDEditSelectedMsm_Func() {
            DisableBaseButtons( true );
            var wnd = new MsmCreator();
            wnd.SetMsm( SelectedMsm );
            wnd.Closing += MsmCreator_OnClosing;
            wnd.Topmost = true;
            wnd.Show();
        }

        private async void CMDExit_Func() {
            if( IsNotSaveChanges ) {
                var answer =
                    MessageBox.Show(
                        string.Format( "������� ������������� ��������� �: {0}. ���������?", User.UserLong() ),
                        "��������������",
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel );
                if( answer == MessageBoxResult.Yes ) {
                    CMDSaveOpenUser_Func();
                } else if( answer == MessageBoxResult.Cancel ) {
                    return;
                } else if( answer == MessageBoxResult.No ) {
                    IsBusy = true;
                    await Task.Factory.StartNew( () => { User.Restore(); } );
                    IsBusy = false;
                }
            }
            if( Parent != null )
                Parent.Close();
        }

        private void CMDExportCSV_Func() {
            using( var sfd = new SaveFileDialog() ) {
                sfd.DefaultExt = "*.csv";
                sfd.Filter = "*.csv|*.csv";
                if( sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK ) {
                    try {
                        SelectedMsm.Msm2CSV( sfd.FileName );
                        MessageBox.Show( "��������� �������� � ���� " + sfd.FileName, "���������", MessageBoxButton.OK );
                    } catch( Exception ex ) {
                        MessageBox.Show( "������ ������ �����", "������", MessageBoxButton.OK, MessageBoxImage.Error );
                    }
                }
            }
        }

        private void CMDFocusChanged_Func() {
            NotifyPropertyChanged( m => m.IsMarkersShow );
        }

        private void CMDNewMsm_Func() {
            DisableBaseButtons( true );

            var wnd = WindowManager.GetWindow<MsmCreator>( mNewMsmWindowID ) ??
                      WindowManager.NewWindow<MsmCreator>( mNewMsmWindowID );


            wnd.Closing += MsmCreator_OnClosing;
            wnd.Topmost = true;
            wnd.Show();

            //if( wnd.ShowDialog() == true ) {
            //    if( wnd.Result != null ) {
            //        var msm = wnd.Result;
            //        User.AddMsm( msm );
            //        ResetList( msm );
            //    }
            //}
        }

        private void CMDOxyLoad_Func( object param ) {
            if( param is PlotViewEx == false ) return;
            var pve = (PlotViewEx) param;
            if( mGraphics.ContainsKey( pve.Tag ) ) {
                mGraphics.Remove( pve.Tag );
            }
            mGraphics.Add( pve.Tag, pve );
        }

        private void CMDOxyPanelLoaded_Func() {
            // ������ ������� ���������� ������ ��� ������ ��������
            CMDOxyPanelLoaded.CanExecute = false;
            CMDUpdateOxy_Func();
        }

        private void CMDOxyUnload_Func( object param ) {
            if( param is PlotViewEx == false ) return;
            var pve = (PlotViewEx) param;
            if( pve.Tag != null )
                if( mGraphics.ContainsKey( pve.Tag ) ) {
                    mGraphics.Remove( pve.Tag );
                }
        }

        private void CMDRemoveSelectedMsm_Func() {
            Debug.Assert( SelectedMsm != null );
            if( IsMsm ) {
                var answer =
                    MessageBox.Show(
                        string.Format(
                            "��������� \"{0}\" ����� ������� ������ �� ���� ���������� ����������� . ������� ���������?",
                            SelectedMsm.Title ), "��������������",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No );
                if( answer == MessageBoxResult.Yes ) {
                    var index = SelectedMsmIndex;
                    User.RemoveMsm( SelectedMsm );
                    ResetList( index );
                    IsNotSaveChanges = true;
                }
            }
        }

        private void CMDResetMsmList_Func() {
            ResetList( SelectedMsm );
        }

        private void CMDResultShow_Func() {
            var wnd = WindowManager.GetWindow<ResultWindow>( mResultWindowID );
            if( wnd == null ) {
                wnd = WindowManager.NewWindow<ResultWindow>( mResultWindowID );
                wnd.SetMsmCollection( User.Msms );
            }
            wnd.Show();
        }

        private void CMDSaveOpenUser_Func() {
            User.SaveDefaultPath();
            IsNotSaveChanges = false;
        }

        private void CMDShowMarkers_Func() {
            if( !Markers.IsWindow ) {
                var wnd = Markers.GetWindow();
                wnd.Closed += ( sender, args ) => { NotifyPropertyChanged( m => m.IsMarkersShow ); };
                wnd.Show();
            } else {
                Markers.CloseWindow();
            }
        }

        private void CMDShowMarkersSettings_Func() {
            var ms = new MarkersSet();
            if( ms.ShowDialog() == true ) {
                var needClose = !Markers.IsWindow;

                var wnd = Markers.GetWindow();
                wnd.UpdateSettings();
                wnd.ReDraw();
                if( needClose ) wnd.FreeWindow();
            }
        }

        private void CMDSpectrumCalc_Func() {
            var first = User.Msms.First( msm => SelectedMsm.ID.Equals( msm.ID ) );
            if( first == null ) return;
            IsBusy = true;
            first.Data.SpectrumAnalys( null, ( data, b ) => {
                Parent.Dispatcher.BeginInvoke( DispatcherPriority.Normal, new Action( () => {
                    //MessageBox.Show( "������ ��������!" );
                    ResetList( first );
                    CMDUpdateOxy_Func();
                    IsBusy = false;
                    IsNotSaveChanges = true;
                } ) );
            }, true );
            TData.StartCalc();
        }

        private Task CMDSpectrumCalc_Func_Async() {
            var first = User.Msms.First( msm => SelectedMsm.ID.Equals( msm.ID ) );
            if( first == null ) return null;
            IsBusy = true;
            var task = Task.Factory.StartNew( () => {
                var exit = false;
                first.Data.SpectrumAnalys( null, ( data, b ) => { exit = true; }, true );
                TData.StartCalc();
                while( exit == false ) {
                    Thread.Sleep( 10 );
                }
                Parent.Dispatcher.BeginInvoke( DispatcherPriority.Normal, new Action( () => {
                    ResetList( first );
                    CMDUpdateOxy_Func();
                    IsBusy = false;
                } ) );
            } );

            return task;
        }

        private void CMDUpdateOxy_Func() {
            var list = mGraphics.Select( kv => kv.Value ).ToList();
            foreach( var value in list ) {
                OxyDraw( value );
            }
        }

        private void DisableBaseButtons( bool b ) {
            CMDNewMsm.CanExecute = !b;
            CMDEditSelectedMsm.CanExecute = !b;
            CMDRemoveSelectedMsm.CanExecute = !b;
            CMDExit.CanExecute = !b;
        }

        private async void MsmCreator_OnClosing( object sender, CancelEventArgs cancelEventArgs ) {
            IsBusy = true;
            var wnd = sender as MsmCreator;
            if( wnd.NotDialogButResult == true ) {
                if( wnd.Result != null ) {
                    if( SelectedMsm != null && SelectedMsm.ID.Equals( wnd.Result.ID ) ) {
                        Cloner.CopyAllFields( SelectedMsm, wnd.Result );
                        Cloner.CopyAllProperties( SelectedMsm, wnd.Result );
                        SelectedMsm.SetData( wnd.Result.Data );
                    } else {
                        User.AddMsm( wnd.Result );
                    }
                    IsNotSaveChanges = true;
                    SelectedMsm = wnd.Result;
                    await CMDSpectrumCalc_Func_Async();

                    wnd.PostSave();
                    wnd.PostScript();
                }
            }
            DisableBaseButtons( false );
            IsBusy = false;
        }

        private async void OxyDraw( PlotViewEx pve ) {
            var command = pve.Tag as string;
            if( command == null ) return;

            await Task.Factory.StartNew( () => {
                pve.Clear();

                var data = (SelectedMsm != null) ? SelectedMsm.Data : new TData();

                if( command.Equals( "all_tremor" ) ) {
                    var thickness = 2;
                    Color? color = null;
                    pve.AddLineSeries( data.GetTremor( Hands.Left ).GetPartPercent( 100 ), color: color,
                        thickness: thickness );
                    pve.AddLineSeries( data.GetTremor( Hands.Right ).GetPartPercent( 100 ), color: color,
                        thickness: thickness );
                } else if( command.Equals( "all_const" ) ) {
                    var thickness = 2;
                    Color? color = null;
                    pve.AddLineSeries( data.GetConst( Hands.Left ).GetPartPercent( 100 ), color: color,
                        thickness: thickness );
                    pve.AddLineSeries( data.GetConst( Hands.Right ).GetPartPercent( 100 ), color: color,
                        thickness: thickness );
                } else if( command.Equals( "all_spectrum" ) ) {
                    var thickness = 2;
                    Color? color = null;
                    pve.AddLineSeries( data.GetSpectrum( Hands.Left ).GetPartPercent( 100 ), color: color,
                        thickness: thickness );
                    pve.AddLineSeries( data.GetSpectrum( Hands.Right ).GetPartPercent( 100 ), color: color,
                        thickness: thickness );
                } else if( command.Equals( "all_corr" ) ) {
                    var thickness = 2;
                    Color? color = null;
                    pve.AddLineSeries( data.GetCorrelation( Hands.Left ).GetPartPercent( 100 ), color: color,
                        thickness: thickness );
                    pve.AddLineSeries( data.GetCorrelation( Hands.Right ).GetPartPercent( 100 ), color: color,
                        thickness: thickness );
                }
            } );
            pve.ReDraw();
        }

        private void ResetList( int selectIndex = 0 ) {
            UpdateAllProperties();

            var toSelect = selectIndex;
            if( selectIndex >= Msms.Count ) {
                toSelect = (Msms.Count - 1);
            }
            SelectedMsmIndex = toSelect;
        }

        private void ResetList( Msm selectItem = null ) {
            UpdateAllProperties();

            if( selectItem == null ) {
                SelectedMsm = Msms.FirstOrDefault();
            } else {
                SelectedMsm = Msms.FirstOrDefault( msm => msm.ID.Equals( selectItem.ID ) );
            }
        }

        private async void UpdateSelectedMsm() {
            IsBusy = true;
            await Task<object>.Factory.StartNew( () => {
                NotifyPropertyChanged( m => m.SelectByIndex );
                NotifyPropertyChanged( m => m.SelectedMsmIndex );
                return null;
            } );
            IsBusy = false;
        }

        public class DataToShow {
            public string Title { get; set; }
            public string ToolTip { get; set; }
            public double V1 { get; set; }
            public double V2 { get; set; }

            public DataToShow( string title, string toolTip, double v1, double v2 ) {
                Title = title;
                ToolTip = toolTip;
                V1 = v1;
                V2 = v2;
            }
        }
    }
}