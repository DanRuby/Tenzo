using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace tEngine.TActual {
    public class Constants : BConstants {
        public const int DEVICE_ID = 22;
        public const string MSM_EXT = ".tam";
        public const string PlayListExt = ".tapl";
        public const string USER_EXT = ".tau";
        public new static string AppDataFolder { get; set; }
        public new static string ApplicationFolder { get; set; }

        public Constants() {
            AppDataFolder = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) + "\\TenzoActual";
            ApplicationFolder = Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ) + "\\.TenzoActual";
            base.AppDataFolder = AppDataFolder;
            base.ApplicationFolder = ApplicationFolder;
            CreateDirectory( AppDataFolder );
            CreateDirectory( ApplicationFolder, true );
        }
    }

    public class Constants1 : IConstants {
        private static readonly string mAppDataFolder =
            Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) + "\\TenzoActual";

        private static readonly string mApplicationFolder =
            Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ) + "\\.TenzoActual";

        private static string mAppSetting = ApplicationFolder + "\\app.settings";
        private static readonly string mMarkersSettings = ApplicationFolder + "\\mark.settings";
        private static string mMsmExt = ".tam";
        private static readonly string mPlayListExt = ".tapl";
        private static readonly string mPLImageFolder = "Image";
        private static readonly string mUserExt = ".tau";

        public static string AppDataFolder {
            get { return mAppDataFolder; }
        }

        public static string ApplicationFolder {
            get { return mApplicationFolder; }
        }

        public static string AppSetting {
            get { return mAppSetting; }
        }

        public static string MarkersSettings {
            get { return mMarkersSettings; }
        }

        public static string MsmExt {
            get { return mMsmExt; }
        }

        public static string PlayListExt {
            get { return mPlayListExt; }
        }

        public static string PlImageFolder {
            get { return mPLImageFolder; }
        }

        public static string UserExt {
            get { return mUserExt; }
        }

        string IConstants.AppSetting {
            get { return AppSetting; }
        }

        string IConstants.MarkersSettings {
            get { return MarkersSettings; }
        }

        static Constants1() {
            CreateDirectory( ApplicationFolder );
            CreateDirectory( AppDataFolder );
        }

        private static void CreateDirectory( string path ) {
            var dinfo = new DirectoryInfo( path );
            if( dinfo.Exists == false )
                dinfo.Create();
        }
    }
}