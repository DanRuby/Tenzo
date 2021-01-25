using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using tEngine.Helpers;

namespace tEngine.TActual.DataModel
{
    [DataContract]
    public class Measurement
    {

        public const string FOLDER_KEY = "LastMsmFolder";
        public PlayList PlayList { get; set; }
        public string FilePath { get; set; }

        public string Comment
        {
            get { return mComment; }
            set
            {
                mComment = value;
                if (PlayList != null)
                    PlayList.IsNotSaveChanges = true;
            }
        }

        public DateTime CreateTime
        {
            get { return mCreateTime; }
            set
            {
                mCreateTime = value;
                if (PlayList != null)
                    PlayList.IsNotSaveChanges = true;
            }
        }

        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(FilePath) == false)
                {
                    FileInfo finfo = new FileInfo(FilePath);
                    if (finfo.Exists)
                        return Path.GetFileNameWithoutExtension(finfo.Name);
                }
                return "Новое измерение";
            }
        }

        public string FIO
        {
            get { return mFio; }
            set
            {
                mFio = value;
                if (PlayList != null)
                    PlayList.IsNotSaveChanges = true;
            }
        }

        public string Theme
        {
            get { return mTheme; }
            set
            {
                mTheme = value;
                if (PlayList != null)
                    PlayList.IsNotSaveChanges = true;
            }
        }

        public string Title
        {
            get { return mTitle; }
            set
            {
                mTitle = value;
                if (PlayList != null)
                    PlayList.IsNotSaveChanges = true;
            }
        }

        public Measurement(Measurement msm)
        {
            Comment = msm.Comment;
            CreateTime = msm.CreateTime;
            FilePath = msm.FilePath;
            Title = msm.Title;
            Theme = msm.Theme;
            FIO = msm.FIO;
            PlayList = new PlayList(msm.PlayList);
        }

        public Measurement()
        {
            CreateTime = DateTime.Now;
            PlayList = new PlayList();
            FilePath = null;
            PlayList.IsNotSaveChanges = false;
        }

        public static Measurement CreateTestMsm(int? maxCount = null)
        {
            // забивате maxCount рисунков из папки рисунков
            string myPicturesFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            DirectoryInfo dinfo = new DirectoryInfo(myPicturesFolder + @"\tenzoTest");
            if (dinfo.Exists == false)
                dinfo = new DirectoryInfo(myPicturesFolder.ToString());

            string[] codecs = ImageCodecInfo.GetImageEncoders().Select(info => info.FilenameExtension).ToArray();
            string extensions = string.Join(";", codecs);
            System.Collections.Generic.IEnumerable<Slide> slides = dinfo.EnumerateFiles()
                .Where(info => extensions.Contains(info.Extension.ToUpper()))
                .Where((info, i) => (i < maxCount) || maxCount == null)
                .Select(
                    (info, i) =>
                    {
                        Slide slide = new Slide();
                        slide.Name = "Картинка №" + (i + 1);
                        slide.Index = i;
                        slide.UriLoad(new Uri(info.FullName));
                        return slide;
                    });

            Measurement msm = new Measurement();
            msm.PlayList.AddRangeSlide(slides);
            return msm;
        }

        public void LoadImage()
        {
            foreach (Slide slide in PlayList.Slides)
            {
                slide.UriLoad();
            }
        }

        public static bool Open(string filePath, out Measurement msm)
        {
            try
            {
                byte[] bytes;
                bool result = FileIO.ReadBytes(filePath, out bytes);
                msm = new Measurement();
                if (result == true)
                {
                    if (msm.LoadFromArray(bytes) == false)
                        throw new Exception("Не удается прочитать файл");
                }

                //var result = FileIO.ReadText( filePath, out json );
                //msm = JsonConvert.DeserializeObject<Msm>( json );

                msm.FilePath = filePath;
                DirectoryInfo folder = new FileInfo(msm.FilePath).Directory;
                if (folder != null)
                    AppSettings.SetValue(Measurement.FOLDER_KEY, folder.FullName);

                // TODO Проверить как будет грузить не существующий файл
                msm.LoadImage();

                msm.PlayList.IsNotSaveChanges = false;
                return result;
            }
            catch (Exception)
            {
                msm = null;
                return false;
            }
        }

        public bool Save()
        {
            string filepath = string.IsNullOrEmpty(FilePath) ? DefaultPath(this) : FilePath;
            return Save(filepath);
        }

        public bool Save(string filePath)
        {
            try
            {
                //var settings = new JsonSerializerSettings {ContractResolver = new JSONContractResolver()};
                //var json = JsonConvert.SerializeObject( this, settings );
                //FileIO.WriteText( filePath, json );
                FileIO.WriteBytes(filePath, this.ToByteArray());
            }
            catch (Exception)
            {

            }
            return true;
        }

        private string DefaultPath(Measurement msm)
        {
            string cDirectory = AppSettings.GetValue(Measurement.FOLDER_KEY, Constants.AppDataFolder);
            string filepath = cDirectory.CorrectSlash();
            filepath += msm.Title + Constants.MSM_EXT;
            return filepath;
        }

        #region Byte <=> Object

        public byte[] ToByteArray()
        {
            byte[] obj = BytesPacker.JSONObj(this);
            byte[] playList = PlayList.ToByteArray();

            return BytesPacker.PackBytes(obj, playList);
        }

        public bool LoadFromArray(byte[] array)
        {
            byte[][] objData = BytesPacker.UnpackBytes(array);
            if (objData.Length != 2) return false;

            Measurement obj = BytesPacker.LoadJSONObj<Measurement>(objData[0]);
            this.Comment = obj.Comment;
            this.CreateTime = obj.CreateTime;
            this.Title = obj.Title;

            bool pl = PlayList.LoadFromArray(objData[1]);

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