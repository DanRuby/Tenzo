using System;

namespace tEngine.TMeter
{
    public class Constants : BConstants
    {
        public const int DEVICE_ID = 11;
        public const string RSCH_EXT = ".tmr";
        public const string USER_EXT = ".tmu";
        public new static string AppDataFolder { get; set; }
        public static string AppImageFolder { get; set; }
        public new static string ApplicationFolder { get; set; }

        public static string UsersFolder
        {
            get { return AppDataFolder + @"\Users"; }
        }

        public Constants()
        {
            AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TenzoMeter";
            ApplicationFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.TenzoMeter";
            AppImageFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TenzoMeter\\Images";
            base.AppDataFolder = AppDataFolder;
            base.ApplicationFolder = ApplicationFolder;
            CreateDirectory(AppDataFolder);
            CreateDirectory(ApplicationFolder, true);
            CreateDirectory(AppImageFolder);
        }
    }

   /* public class Constants1 : IConstants
    {
        private static readonly string mAppDataFolder =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TenzoMeter";

        private static readonly string mApplicationFolder =
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.TenzoMeter";

        private static readonly string mAppSetting = ApplicationFolder + "\\app.settings";
        private static readonly string mGUISettings = ApplicationFolder + "\\gui.settings";
        private static readonly string mMarkersSettings = ApplicationFolder + "\\mark.settings";
        private static readonly string mMSMGettings = ApplicationFolder + "\\msm.settings";
        private static readonly string mRschExt = ".rtz";
        private static readonly string mUserExt = ".utz";

        public static string AppDataFolder
        {
            get { return mAppDataFolder; }
        }

        public static string ApplicationFolder
        {
            get { return mApplicationFolder; }
        }

        public static string AppSetting
        {
            get { return mAppSetting; }
        }

        public static string GuiSettings
        {
            get { return mGUISettings; }
        }

        public static string GuiSettings1
        {
            get { return mGUISettings; }
        }

        public static string MarkersSettings
        {
            get { return mMarkersSettings; }
        }

        public static string MsmGettings
        {
            get { return mMSMGettings; }
        }

        public static string MsmGettings1
        {
            get { return mMSMGettings; }
        }

        public static string RschExt
        {
            get { return mRschExt; }
        }

        public static string UserExt
        {
            get { return mUserExt; }
        }

        string IConstants.AppSetting
        {
            get { return AppSetting; }
        }

        string IConstants.MarkersSettings
        {
            get { return MarkersSettings; }
        }

        static Constants1()
        {
            CreateDirectory(ApplicationFolder, true);
            CreateDirectory(AppDataFolder);
        }

        private static void CreateDirectory(string path, bool hidden = false)
        {
            DirectoryInfo dinfo = new DirectoryInfo(path);
            if (dinfo.Exists == false)
            {
                dinfo.Create();
                dinfo.Attributes = FileAttributes.Directory;
                if (hidden) dinfo.Attributes |= FileAttributes.Hidden;
            }
        }
    }*/
}