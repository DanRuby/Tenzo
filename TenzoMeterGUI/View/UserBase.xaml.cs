using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using tEngine.Helpers;
using tEngine.MVVM;
using tEngine.Recorder;
using tEngine.TMeter;
using tEngine.TMeter.DataModel;

namespace TenzoMeterGUI.View {
    /// <summary>
    /// Interaction logic for UserBase.xaml
    /// </summary>
    public partial class UserBase : Window {
        private UserBaseVM mDataContext;

        public UserBase() {
            InitializeComponent();
            WindowManager.UpdateWindowPos( this.GetType().Name, this );
            mDataContext = new UserBaseVM() {Parent = this};
            DataContext = mDataContext;
        }

        private void UsersListView_OnMouseDoubleClick( object sender, MouseButtonEventArgs e ) {
            mDataContext.CMDOpenUser.DoExecute( null );
        }

        private void Window_OnClosing( object sender, CancelEventArgs e ) {
            if( mDataContext != null ) {
                try {
                    DialogResult = mDataContext.DialogResult;
                } catch( Exception ex ) {
                    //Debug.Assert( false, ex.Message );
                }
            }
            Device.AbortAll();
            Markers.CloseWindow();
            WindowManager.CloseAll();
            WindowManager.SaveWindowPos( this.GetType().Name, this );
        }
    }


    public class UserBaseVM : Observed<UserBaseVM> {
        private User mSelectedUser;
        private int mSelectedUserIndex;
        private bool mShowBagel;
        private ObservableCollection<User> mUserList;

        public bool CanUserOpen {
            get { return (SelectedUser != null) && CMDOpenUser.CanExecute; }
        }

        public Command CMDAddNew { get; private set; }
        public Command CMDEditSelected { get; private set; }
        public Command CMDOpenUser { get; private set; }
        public Command CMDRemoveSelected { get; private set; }
        public Command CMDResetList { get; private set; }

        public string DeviceStatus {
            get {
                if( IsDesignMode ) return "DesignMode";
                var status = Device.GetDevice( Constants.DEVICE_ID ).DeviceState;
                var msg = "";
                switch( status ) {
                    case DeviceStates.AllRight:
                        msg = "Есть связь с устройством";
                        break;
                    case DeviceStates.DemoMode:
                        msg = "Программа работает в демонстрационном режиме";
                        break;

                    case DeviceStates.DllNotFind:
                        msg = "Не удается найти библиотеку TenzoDevice.dll";
                        break;
                    default:
                        msg = "Нет связи с устройством";
                        break;
                }
                return msg;
            }
        }

        public Msm SelectedMsm { get; set; }

        public User SelectedUser {
            get { return mSelectedUser; }
            set {
                mSelectedUser = value;
                NotifyPropertyChanged( m => m.SelectedUser );
                NotifyPropertyChanged( m => m.CanUserOpen );
            }
        }

        public int SelectedUserIndex {
            get { return mSelectedUserIndex; }
            set {
                mSelectedUserIndex = value;
                NotifyPropertyChanged( m => m.SelectedUserIndex );
            }
        }

        public bool ShowBagel {
            get { return mShowBagel; }
            set {
                mShowBagel = value;
                NotifyPropertyChanged( m => m.ShowBagel );
            }
        }

        public string Title {
            get {
                var assembly = Assembly.GetExecutingAssembly();
                var fvi = FileVersionInfo.GetVersionInfo( assembly.Location );
                var version = fvi.FileMajorPart + "." + fvi.FileMinorPart + ".";
                return "ТензоМетер v." + version;
            }
        }

        public ObservableCollection<User> UserList {
            get {
                if( mUserList == null )
                    mUserList = new ObservableCollection<User>( OpenUserList() );
                return mUserList;
            }
        }

        public UserBaseVM() {
            if( !IsDesignMode ) {
                Device.CreateDevice( Constants.DEVICE_ID );
            }
            CMDAddNew = new Command( CMDAddNew_Func );
            CMDRemoveSelected = new Command( CMDRemoveSelected_Func );
            CMDResetList = new Command( CMDResetListCommand_Func );
            CMDEditSelected = new Command( CMDEditSelected_Func );
            CMDOpenUser = new Command( CMDOpenUser_Func );

            // таймер обновления состояния устройства
            var stateUpdate = new DispatcherTimer();
            stateUpdate.Interval = new TimeSpan( 0, 0, 0, 0, 100 );
            stateUpdate.Tick += ( sender, args ) => { NotifyPropertyChanged( m => m.DeviceStatus ); };
            stateUpdate.Start();


            SelectedUser = UserList.FirstOrDefault();
            SelectedUserIndex = 0;
            UpdateAllProperties();
        }

        private bool AddUserToBase( User user ) {
            if( user == null ) return false;
            user.SaveDefaultPath();
            UserList.Add( user );
            return true;
        }

        private void CMDAddNew_Func() {
            var uid = new UserInfo();
            uid.EditMode = true;
            if( uid.ShowDialog() == true ) {
                if( uid.Result != null ) {
                    AddUserToBase( uid.Result );
                    ResetList( uid.Result );
                }
            }
        }

        private async void CMDEditSelected_Func() {
            Disable( true );

            var uid = new UserInfo();
            await Task<object>.Factory.StartNew( () => {
                uid.EditMode = false;
                uid.SetUser( SelectedUser );
                return null;
            } );
            ShowBagel = false;


            if( uid.ShowDialog() == true ) {
                ShowBagel = true;
                await Task<object>.Factory.StartNew( () => {
                    if( uid.Result != null ) {
                        AddUserToBase( uid.Result );
                        ResetList( uid.Result );
                        var windows =
                            WindowManager.GetOpenWindows<UserWorkSpace>()
                                .Select( window => (UserWorkSpace) window )
                                .Where( uws => uws.ID.Equals( SelectedUser.ID ) );
                        if( windows.Any() ) {
                            windows.First().CopyUserInfo( uid.Result );
                        }
                    }
                    return null;
                } );
                ShowBagel = false;
            }

            Disable( false );
        }

        private async void CMDOpenUser_Func() {
            Disable( true );

            if( SelectedUser != null ) {
                var windows =
                    WindowManager.GetOpenWindows<UserWorkSpace>()
                        .Select( window => (UserWorkSpace) window )
                        .Where( uws => uws.ID.Equals( SelectedUser.ID ) );
                if( windows.Any() == false ) {
                    var wnd = WindowManager.NewWindow<UserWorkSpace>();
                    await Task<string>.Factory.StartNew( () => {
                        wnd.SetUser( SelectedUser );
                        return "";
                    } );
                    wnd.OpenMsm( SelectedMsm );
                    wnd.Closed += ( sender, args ) => { CMDResetListCommand_Func(); };
                    wnd.Show();
                } else {
                    var wnd = windows.First();
                    wnd.Show();
                    wnd.Topmost = true;
                    wnd.Topmost = false;
                    wnd.Focus();
                }
            }

            Disable( false );
        }

        private void CMDRemoveSelected_Func() {
            Debug.Assert( SelectedUser != null );
            var answer =
                MessageBox.Show(
                    string.Format( "Вся информация о пациенте \"{0}\" будет удалена. Удалить пациента из базы?",
                        SelectedUser.UserLong() ), "Предупреждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No );
            if( answer == MessageBoxResult.Yes ) {
                var index = SelectedUserIndex;
                RemoveUserFromBase( SelectedUser );
                ResetList( index );
            }
        }

        private void CMDResetListCommand_Func() {
            ResetList( SelectedUser );
        }

        private void Disable( bool b ) {
            CMDEditSelected.CanExecute = !b;
            CMDOpenUser.CanExecute = !b;
            ShowBagel = b;
            NotifyPropertyChanged( m => m.CMDEditSelected.CanExecute );
            NotifyPropertyChanged( m => m.CMDOpenUser.CanExecute );
        }

        private IEnumerable<User> OpenUserList() {
            if( IsDesignMode ) {
                return new List<User>() {
                    new User() {Name = "Pasha", SName = "Burenev", FName = "Nikolaevich"},
                    new User() {Name = "Ivan", SName = "Burenev", FName = "Nikolaevich"},
                    new User() {
                        Name = "Evdok",
                        SName = "Zam",
                        FName = "Dekan",
                        Comment =
                            @"Cannot find resource named '{serviceLocator}'. Resource names are case sensitive. Error at object 'System.Windows.Data.Binding' in markup file 'WpfApp;component/mainwindow.xaml' Line 8 Position 45.Cannot find resource named '{serviceLocator}'. Resource names are case sensitive. Error at object 'System.Windows.Data.Binding' in markup file 'WpfApp;component/mainwindow.xaml' Line 8 Position 45."
                    }
                };
            }
            var list = new List<User>();
            var files = User.GetDefaultFiles();
            foreach( var finfo in files ) {
                User user;
                if( User.Open( finfo.FullName, out user ) ) {
                    list.Add( user );
                }
            }
            return list;
        }

        private bool RemoveUserFromBase( User user ) {
            if( user == null ) return false;
            UserList.Remove( user );
            var finfo = new FileInfo( user.FilePath );
            if( finfo.Exists ) {
                finfo.Delete();
                return true;
            }
            return false;
        }

        private async void ResetList( int selectIndex = 0 ) {
            Disable( true );
            await Task.Factory.StartNew( () => {
                var toSelect = selectIndex;
                if( selectIndex >= UserList.Count ) {
                    toSelect = (UserList.Count - 1);
                }
                UpdateAllProperties();
                SelectedUserIndex = toSelect;
            } );
            Disable( false );
        }

        private async void ResetList( User selectItem = null ) {
            Disable( true );
            await Task.Factory.StartNew( () => {
                //mUserList = null;
                if( selectItem == null ) {
                    SelectedUser = UserList.FirstOrDefault();
                } else {
                    SelectedUser = UserList.FirstOrDefault( user => user.ID.Equals( selectItem.ID ) );
                }
                UpdateAllProperties();
            } );
            Disable( false );
        }
    }
}