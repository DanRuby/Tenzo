using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using tEngine.Helpers;

namespace tEngine.TMeter.DataModel
{
    [DataContract]
    public class User {
        public const string FOLDER_KEY = "LastUserFolder";
        private List<Measurement> mMsms;

        public string BirthDateString {
            get {
                var date = BirthDate.ToString( "dd" );
                var month = BirthDate.ToString( "MMMM", new CultureInfo( "ru-ru" ) );
                var year = BirthDate.ToString( "yyyy" );
                return date + "/" + month + "/" + year;
            }
        }

        public string FilePath { get; set; }
        public bool IsNotSaveChanges { get; set; }

        public ObservableCollection<Measurement> Msms {
            get { return new ObservableCollection<Measurement>( mMsms ); }
        }

        public User() => Init();

        public User( User user ) {
            Init();
            LoadFromArray( user.ToByteArray() );
            return;
            // todo проверить все ли копируется
            /*var pinfo = user.GetType().GetProperties();
            pinfo.ToList().ForEach( info => {
                if( info.CanRead && info.CanWrite ) {
                    info.SetValue( this, info.GetValue( user, null ), null );
                }
            } );
            mMsms = user.mMsms;*/
        }

        public void AddMsm( Measurement msm ) {
            msm.UserAssociated( this );
            mMsms.Add( msm );
            IsNotSaveChanges = true;
        }

        public static IEnumerable<FileInfo> GetDefaultFiles() {
            var dinfo = new DirectoryInfo( Constants.UsersFolder );
            if( dinfo.Exists == false )
                dinfo.Create();
            return dinfo.GetFiles().Where( finfo => finfo.Extension.Equals( Constants.USER_EXT ) );
        }

        public Measurement GetMsm( Guid msmId ) {
            var msms = mMsms.Where( msm => msm.ID.Equals( msmId ) );
            var enumerable = msms as Measurement[] ?? msms.ToArray();
            return enumerable.Any() ? enumerable[0] : null;
        }

        public Measurement GetMsm( int index ) {
            if( mMsms.Count > index )
                return mMsms[index];
            return null;
        }

        public int GetMsmCount() => mMsms.Count;

        public IEnumerable<Measurement> GetMsms() => mMsms;

        public static User GetTestUser( string name = "Имя", int msmCount = 10 ) {
            var sb = new StringBuilder();
            for( int i = 0; i < 1; i++ ) {
                sb.Append( "Тестовый пациент" );
                if( i%10 == 0 )
                    sb.Append( "\r\n" );
            }
            var testUser = new User {
                Name = name,
                SName = "Фамилия",
                FName = "Отчество",
                BirthDate = DateTime.Now,
                Comment = sb.ToString()
            };
            for( var i = 0; i < msmCount; i++ ) {
                testUser.AddMsm( Measurement.GetTestMsm( testUser, "Измерение " + (i + 1) ) );
            }
            return testUser;
        }

        public static bool Open( string filePath, out User user ) {
            try {
                byte[] bytes;
                var result = FileIO.ReadBytes( filePath, out bytes );
                user = new User();
                if( result == true ) {
                    if( user.LoadFromArray( bytes ) == false )
                        throw new Exception( "Не удается прочитать файл" );
                }
                user.FilePath = filePath;

                var folder = new FileInfo( user.FilePath ).Directory;
                if( folder != null )
                    AppSettings.SetValue( User.FOLDER_KEY, folder.FullName );

                user.IsNotSaveChanges = false;
                return result;
            } catch( Exception  ) {
                user = new User();
                return false;
            }
        }

        public void RemoveMsm( Measurement msm ) {
            mMsms.Remove( msm );
            IsNotSaveChanges = true;
        }

        public void RemoveMsm( Guid msmId ) {
            var msm = GetMsm( msmId );
            RemoveMsm( msm );
        }

        public bool Restore() {
            byte[] bytes;
            if( FileIO.ReadBytes( this.FilePath, out bytes ) ) {
                IsNotSaveChanges = false;
                return LoadFromArray( bytes );
            }
            return false;
        }

        public bool Save() {
            var filepath = string.IsNullOrEmpty( FilePath ) ? DefaultPath( this ) : FilePath;
            return Save( filepath );
        }

        public bool Save( string filePath ) {
            try {
                //var settings = new JsonSerializerSettings {ContractResolver = new JSONContractResolver()};
                //var json = JsonConvert.SerializeObject( this, settings );
                //FileIO.WriteText( filePath, json );

                FileIO.WriteBytes( filePath, this.ToByteArray() );
            } catch( Exception ex ) {
                Debug.Assert( false, ex.Message );
            }
            return true;
        }

        public bool SaveDefaultPath() {
            var filepath = DefaultPath( this );
            return Save( filepath );
        }

        public string ToString(bool longName = false) => longName ? UserLong() : UserShort();

        public string UserLong() {
            var str = string.Format( "{0} {1} {2}", SName, Name, FName );
            return str;
        }

        public string UserShort() {
            var n = string.IsNullOrEmpty( Name ) ? null : (Name[0] + ".");
            var f = string.IsNullOrEmpty( FName ) ? null : (FName[0] + ".");
            var s = string.IsNullOrEmpty( SName ) ? null : SName + " ";
            var str = string.Format( "{0}{1}{2}", s, n, f );
            return str;
        }

        private string DefaultPath( User user ) {
            // var cDirectory = AppSettings.GetValue( User.FOLDER_KEY, Constants.AppDataFolder );
            var filepath = Constants.UsersFolder.CorrectSlash();
            filepath += user.ID + Constants.USER_EXT;
            return filepath;
        }

        private void Init() {
            ID = Guid.NewGuid();
            BirthDate = DateTime.Now;
            mMsms = new List<Measurement>();
            FilePath = DefaultPath( this );
        }

        #region JSON

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string SName { get; set; }

        [DataMember]
        public string FName { get; set; }

        [DataMember]
        public Guid ID { get; private set; }

        [DataMember]
        public DateTime BirthDate { get; set; }

        [DataMember]
        public string Comment { get; set; }

        #endregion

        #region Byte <=> Object

        public byte[] ToByteArray() {
            var obj = BytesPacker.JSONObj( this );
            var msmData = new byte[mMsms.Count][];
            for( int i = 0; i < msmData.Length; i++ ) {
                var msm = mMsms[i];
                msmData[i] = msm.ToByteArray();
            }
            var msms = BytesPacker.PackBytes( msmData );
            return BytesPacker.PackBytes( obj, msms );
        }

        public bool LoadFromArray( byte[] array ) {
            var objData = BytesPacker.UnpackBytes( array );
            if( objData.Length != 2 ) return false;

            var obj = BytesPacker.LoadJSONObj<User>( objData[0] );
            this.Name = obj.Name;
            this.SName = obj.SName;
            this.FName = obj.FName;
            this.ID = obj.ID;
            this.BirthDate = obj.BirthDate;
            this.Comment = obj.Comment;

            this.mMsms.Clear();
            var msmsArray = BytesPacker.UnpackBytes( objData[1] );
            var result = true;
            foreach( byte[] bytes in msmsArray ) {
                var newMsm = new Measurement();
                result = result && newMsm.LoadFromArray( bytes );
                this.AddMsm( newMsm );
            }
            IsNotSaveChanges = false;
            return result;
        }

        #endregion
    }
}