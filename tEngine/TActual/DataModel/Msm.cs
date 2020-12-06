using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using tEngine.Helpers;

namespace tEngine.TActual.DataModel {
    [DataContract]
    public class Msm {
        public const string FOLDER_KEY = "LastMsmFolder";

        public string Comment {
            get { return mComment; }
            set {
                mComment = value;
                if( PlayList != null ) PlayList.IsNotSaveChanges = true;
            }
        }

        public DateTime CreateTime {
            get { return mCreateTime; }
            set {
                mCreateTime = value;
                if( PlayList != null ) PlayList.IsNotSaveChanges = true;
            }
        }

        public string FileName {
            get {
                if( string.IsNullOrEmpty( FilePath ) == false ) {
                    var finfo = new FileInfo( FilePath );
                    if( finfo.Exists )
                        return Path.GetFileNameWithoutExtension( finfo.Name );
                }
                return "Новое измерение";
            }
        }

        public string FilePath { get; set; }

        public string FIO {
            get { return mFio; }
            set {
                mFio = value;
                if( PlayList != null ) PlayList.IsNotSaveChanges = true;
            }
        }

        public PlayList PlayList { get; set; }

        public string Theme {
            get { return mTheme; }
            set {
                mTheme = value;
                if( PlayList != null ) PlayList.IsNotSaveChanges = true;
            }
        }

        public string Title {
            get { return mTitle; }
            set {
                mTitle = value;
                if( PlayList != null ) PlayList.IsNotSaveChanges = true;
            }
        }

        public Msm( Msm msm ) {
            this.Comment = msm.Comment;
            this.CreateTime = msm.CreateTime;
            this.FilePath = msm.FilePath;
            this.Title = msm.Title;
            this.Theme = msm.Theme;
            this.FIO = msm.FIO;
            this.PlayList = new PlayList( msm.PlayList );
        }

        public Msm() {
            CreateTime = DateTime.Now;
            PlayList = new PlayList();
            FilePath = null;
            PlayList.IsNotSaveChanges = false;
        }

        public static Msm CreateTestMsm( int? maxCount = null ) {
            // забивате maxCount рисунков из папки рисунков
            var pic = Environment.GetFolderPath( Environment.SpecialFolder.MyPictures );
            var dinfo = new DirectoryInfo( pic + @"\tenzoTest" );
            if( dinfo.Exists == false )
                dinfo = new DirectoryInfo( pic.ToString() );

            var codecs = ImageCodecInfo.GetImageEncoders().Select( info => info.FilenameExtension ).ToArray();
            var extensions = string.Join( ";", codecs );
            var slides = dinfo.EnumerateFiles()
                .Where( info => extensions.Contains( info.Extension.ToUpper() ) )
                .Where( ( info, i ) => (i < maxCount) || maxCount == null )
                .Select(
                    ( info, i ) => {
                        var slide = new Slide();
                        slide.Name = "Картинка №" + (i + 1);
                        slide.Index = i;
                        slide.UriLoad( new Uri( info.FullName ) );
                        return slide;
                    } );

            var msm = new Msm();
            msm.PlayList.AddRangeSlide( slides );
            return msm;
        }

        public void LoadImage() {
            foreach( var slide in PlayList.Slides ) {
                slide.UriLoad();
            }
        }

        public static bool Open( string filePath, out Msm msm ) {
            try {
                byte[] bytes;
                var result = FileIO.ReadBytes( filePath, out bytes );
                msm = new Msm();
                if( result == true ) {
                    if( msm.LoadFromArray( bytes ) == false )
                        throw new Exception( "Не удается прочитать файл" );
                }

                //var result = FileIO.ReadText( filePath, out json );
                //msm = JsonConvert.DeserializeObject<Msm>( json );

                msm.FilePath = filePath;
                var folder = new FileInfo( msm.FilePath ).Directory;
                if( folder != null )
                    AppSettings.SetValue( Msm.FOLDER_KEY, folder.FullName );

                // TODO Проверить как будет грузить не существующий файл
                msm.LoadImage();

                msm.PlayList.IsNotSaveChanges = false;
                return result;
            } catch( Exception ex ) {
                msm = null;
                return false;
            }
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
                ex = ex;
            }
            return true;
        }

        private string DefaultPath( Msm msm ) {
            var cDirectory = AppSettings.GetValue( Msm.FOLDER_KEY, Constants.AppDataFolder );
            var filepath = cDirectory.CorrectSlash();
            filepath += msm.Title + Constants.MSM_EXT;
            return filepath;
        }

        #region Byte <=> Object

        public byte[] ToByteArray() {
            var obj = BytesPacker.JSONObj( this );
            var playList = PlayList.ToByteArray();

            return BytesPacker.PackBytes( obj, playList );
        }

        public bool LoadFromArray( byte[] array ) {
            var objData = BytesPacker.UnpackBytes( array );
            if( objData.Length != 2 ) return false;

            var obj = BytesPacker.LoadJSONObj<Msm>( objData[0] );
            this.Comment = obj.Comment;
            this.CreateTime = obj.CreateTime;
            this.Title = obj.Title;

            var pl = PlayList.LoadFromArray( objData[1] );

            return pl;
        }

        #endregion

        #region JSON   

        [DataMember]
        private string mComment;

        [DataMember]
        private string mTitle;

        [DataMember]
        private string mTheme;

        [DataMember]
        private string mFio;

        [DataMember]
        private DateTime mCreateTime;

        #endregion
    }
}