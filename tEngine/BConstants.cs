using System;
using System.IO;

namespace tEngine
{
    /// <summary>
    /// Базовый класс констант
    /// </summary>
    public abstract class BConstants {
        public string AppDataFolder { get; set; }
        public string ApplicationFolder { get; set; }

        public string AppSettings {
            get { return ApplicationFolder + "\\app.settings"; }
        }

        public string MarkersSettings {
            get { return ApplicationFolder + "\\mark.settings"; }
        }

        protected static void CreateDirectory( string path, bool hidden = false ) {
            var dinfo = new DirectoryInfo( path );
            if( dinfo.Exists == false ) {
                dinfo.Create();
            }
            dinfo.Attributes = FileAttributes.Directory;
            if( hidden ) 
                dinfo.Attributes |= FileAttributes.Hidden;
        }
    }

    public class CommonConstants : BConstants {
        public const int DEVICE_ID = 33;
        public new static string AppDataFolder { get; set; }
        public new static string ApplicationFolder { get; set; }

        public CommonConstants() {
            AppDataFolder = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) + "\\Tenzo";
            ApplicationFolder = Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ) + "\\.Tenzo";
            base.AppDataFolder = AppDataFolder;
            base.ApplicationFolder = ApplicationFolder;
            CreateDirectory( AppDataFolder );
            CreateDirectory( ApplicationFolder, true );
        }
    }
}