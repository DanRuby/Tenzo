using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using tEngine.DataModel;
using tEngine.Helpers;
using tEngine.MVVM;
using tEngine.TMeter;
using tEngine.TMeter.DataModel;
using TenzoMeterGUI.ViewModel;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace TenzoMeterGUI.View {
    /// <summary>
    /// Interaction logic for UserInfo.xaml
    /// </summary>
    public partial class UserInfo : Window {
        private UserInfoVM mDataContext;

        public bool EditMode {
            get { return mDataContext != null && mDataContext.EditMode; }
            set { if( mDataContext != null ) mDataContext.EditMode = value; }
        }

        public User Result {
            get { return mDataContext == null ? null : mDataContext.GetUser(); }
        }

        public UserInfo() {
            InitializeComponent();
            WindowManager.UpdateWindowPos( this.GetType().Name, this );
            mDataContext = new UserInfoVM() {Parent = this};
            DataContext = mDataContext;
        }

        public void SetUser( User user ) {
            mDataContext.SetUser( user );
        }

        private void TextBox_KeyDown( object sender, KeyEventArgs e ) {
            if( e.Key == Key.Enter ) {
                var element = sender as UIElement;
                if( element != null ) {
                    element.MoveFocus( new TraversalRequest( FocusNavigationDirection.Next ) );
                }
            }
        }

        private void Window_OnClosing( object sender, CancelEventArgs e ) {
            if( mDataContext != null ) {
                mDataContext.PreClosed();
                try {
                    DialogResult = mDataContext.DialogResult;
                } catch {
                    /*если окно не диалог - вылетит исключение, ну и пусть*/
                }
            }
            WindowManager.SaveWindowPos( this.GetType().Name, this );
        }
    }

    public class UserInfoVM : Observed<UserInfoVM> {
        private string mFileTitle;
        private bool mIsEditableName;
        private User mUser;
        private bool mEditMode;
        public Command CMDBrowse { get; private set; }
        public Command CMDCancel { get; private set; }
        public Command CMDCreate { get; private set; }
        public string CurrentDir { get; set; }
        public Stack<string> DirPaths { get; set; }

        public bool EditMode {
            get { return mEditMode; }
            set {
                //mEditMode = value;
                // всегда можно редактировать
                mEditMode = true;
            }
        }

        public string FileTitle {
            get {
                if( IsEditableName )
                    return mFileTitle;
                return mUser.UserShort();
            }
            set {
                mFileTitle = value;
                NotifyPropertyChanged( m => m.FileTitle );
            }
        }

        public bool IsEditableName {
            get { return mIsEditableName; }
            set {
                // Сохранение того что уже набрано/собрано
                FileTitle = FileTitle;

                mIsEditableName = value;
                NotifyPropertyChanged( m => m.IsEditableName );
            }
        }

        public UserInfoVM() {
            CMDCreate = new Command( Create );
            CMDCancel = new Command( Cancel );
            CMDBrowse = new Command( Browse );

            EditMode = false;

            DirPaths = AppSettings.GetValue( "LastDirPaths", DirPaths ?? new Stack<string>() );
            DirPaths.Push( Constants.AppDataFolder );

            if( IsDesignMode ) {
                SetUser( User.GetTestUser( msmCount: 2 ) );
            } else {
                SetUser( new User() );
            }
        }

        public User GetUser() {
            return mUser;
        }

        public void PreClosed() {
            //
        }

        public void SetUser( User user ) {
            //mUser = new User( user );
            mUser = user;

            var dInfo = new FileInfo( mUser.FilePath ).Directory ?? new DirectoryInfo( Constants.AppDataFolder );
            CurrentDir = dInfo.FullName;
            DirPaths.Push( CurrentDir );

            DirPaths = new Stack<string>( DirPaths.Distinct() );
            NotifyPropertyChanged( m => m.DirPaths );

            // update all properties
            this.GetType()
                .GetProperties()
                .Where( info => info.CanRead )
                .ToList()
                .ForEach( info => { NotifyPropertyChanged( info.Name ); } );
        }

        private void Browse() {
            var ofd = new FolderBrowserDialog();
            var dinfo = new DirectoryInfo( CurrentDir );
            if( dinfo.Exists )
                ofd.SelectedPath = dinfo.FullName + @"\";
            if( ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK ) {
                CurrentDir = ofd.SelectedPath;
            }
        }

        private void Cancel() {
            EndDialog( dialogResult: false );
        }

        private bool CheckValid() {
            var msg = "";
            if( string.IsNullOrWhiteSpace( Name ) ) {
                msg = "Необходимо заполнить поле \"Имя\"";
            } else if( string.IsNullOrWhiteSpace( SName ) ) {
                msg = "Необходимо заполнить поле \"Фамилия\"";
            } else if( string.IsNullOrWhiteSpace( FName ) ) {
                msg = "Необходимо заполнить поле \"Отчество\"";
            } else {
                Name = Name.Trim( ' ' );
                SName = SName.Trim( ' ' );
                FName = FName.Trim( ' ' );
                var validSymbols = new Regex( @"[*. \-_a-zA-Z0-9а-яА-Я]*" );
                var nameBad = validSymbols.Replace( Name, "" );
                var snameBad = validSymbols.Replace( SName, "" );
                var fnameBad = validSymbols.Replace( FName, "" );
                if( nameBad.Length > 0 ) {
                    msg = string.Format( "Поле \"Имя\" содержит недопустимые символы: {0}", nameBad );
                } else if( snameBad.Length > 0 ) {
                    msg = string.Format( "Поле \"Фамилия\" содержит недопустимые символы: {0}", snameBad );
                } else if( fnameBad.Length > 0 ) {
                    msg = string.Format( "Поле \"Отчество\" содержит недопустимые символы: {0}", fnameBad );
                }
            }

            if( !string.IsNullOrWhiteSpace( msg ) ) {
                MessageBox.Show( msg, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error );
                return false;
            }


            return true;
        }

        private void Create() {
            if( EditMode ) {
                if( CheckValid() == false )
                    return;
            }
            DirPaths.Push( CurrentDir );
            AppSettings.SetValue( "LastDirPaths", new Stack<string>( DirPaths.Distinct() ) );
            mUser.FilePath = CurrentDir + @"\" + FileTitle + Constants.USER_EXT;
            EndDialog( dialogResult: true );
        }

        #region User Fields

        public DateTime BirthDate {
            get { return mUser.BirthDate; }
            set {
                mUser.BirthDate = value;
                NotifyPropertyChanged( m => m.BirthDate );
            }
        }

        public string Comment {
            get { return mUser.Comment; }
            set {
                mUser.Comment = value;
                NotifyPropertyChanged( m => m.Comment );
            }
        }

        public string FName {
            get { return mUser.FName; }
            set {
                mUser.FName = value;
                NotifyPropertyChanged( m => m.FName );
                NotifyPropertyChanged( m => m.FileTitle );
            }
        }

        public string Name {
            get { return mUser.Name; }
            set {
                mUser.Name = value;
                NotifyPropertyChanged( m => m.Name );
                NotifyPropertyChanged( m => m.FileTitle );
            }
        }

        public string SName {
            get { return mUser.SName; }
            set {
                mUser.SName = value;
                NotifyPropertyChanged( m => m.SName );
                NotifyPropertyChanged( m => m.FileTitle );
            }
        }

        #endregion User Fields
    }
}