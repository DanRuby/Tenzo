using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public class User
    {
        public const string FOLDER_KEY = "LastUserFolder";
        private List<Measurement> mMsms;

        public string FilePath { get; set; }
        public bool IsNotSaveChanges { get; set; }

        public ObservableCollection<Measurement> Msms
        {
            get { return new ObservableCollection<Measurement>(mMsms); }
        }

        public User() => Init();

        public void AddMsm(Measurement msm)
        {
            msm.Owner=this;
            mMsms.Add(msm);
            IsNotSaveChanges = true;
        }

        public static IEnumerable<FileInfo> GetDefaultFiles()
        {
            DirectoryInfo dinfo = new DirectoryInfo(Constants.UsersFolder);
            if (dinfo.Exists == false)
                dinfo.Create();
            return dinfo.GetFiles().Where(finfo => finfo.Extension.Equals(Constants.USER_EXT));
        }

        public static User GetTestUser(string name = "Имя", int msmCount = 10)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 1; i++)
            {
                sb.Append("Тестовый пациент");
                if (i % 10 == 0)
                    sb.Append("\r\n");
            }
            User testUser = new User
            {
                Name = name,
                SName = "Фамилия",
                FName = "Отчество",
                BirthDate = DateTime.Now,
                Comment = sb.ToString()
            };
            for (int i = 0; i < msmCount; i++)
            {
                testUser.AddMsm(Measurement.GetTestMsm(testUser, "Измерение " + (i + 1)));
            }
            return testUser;
        }

        public static bool Open(string filePath, out User user)
        {
            try
            {
                byte[] bytes;
                bool result = FileIO.ReadBytes(filePath, out bytes);
                user = new User();
                if (result == true)
                {
                    if (user.LoadFromArray(bytes) == false)
                        throw new Exception("Не удается прочитать файл");
                }
                user.FilePath = filePath;

                DirectoryInfo folder = new FileInfo(user.FilePath).Directory;
                if (folder != null)
                    AppSettings.SetValue(User.FOLDER_KEY, folder.FullName);

                user.IsNotSaveChanges = false;
                return result;
            }
            catch (Exception)
            {
                user = new User();
                return false;
            }
        }

        public void RemoveMsm(Measurement msm)
        {
            mMsms.Remove(msm);
            IsNotSaveChanges = true;
        }

        public bool Restore()
        {
            byte[] bytes;
            if (FileIO.ReadBytes(FilePath, out bytes))
            {
                IsNotSaveChanges = false;
                return LoadFromArray(bytes);
            }
            return false;
        }

        public bool SaveDefaultPath()
        {
            string filepath = DefaultPath(this);
            return Save(filepath);
        }

        public string UserLong()
        {
            string str = string.Format("{0} {1} {2}", SName, Name, FName);
            return str;
        }

        public string UserShort()
        {
            string n = string.IsNullOrEmpty(Name) ? null : (Name[0] + ".");
            string f = string.IsNullOrEmpty(FName) ? null : (FName[0] + ".");
            string s = string.IsNullOrEmpty(SName) ? null : SName + " ";
            string str = string.Format("{0}{1}{2}", s, n, f);
            return str;
        }


        private string DefaultPath(User user)
        {
            string filepath = Constants.UsersFolder.CorrectSlash();
            filepath += user.ID + Constants.USER_EXT;
            return filepath;
        }

        private void Init()
        {
            ID = Guid.NewGuid();
            BirthDate = DateTime.Now;
            mMsms = new List<Measurement>();
            FilePath = DefaultPath(this);
        }

        private Measurement GetMsm(Guid msmId)
        {
            IEnumerable<Measurement> msms = mMsms.Where(msm => msm.ID.Equals(msmId));
            Measurement[] enumerable = msms as Measurement[] ?? msms.ToArray();
            return enumerable.Any() ? enumerable[0] : null;
        }

        private bool Save(string filePath)
        {
            try
            {
                FileIO.WriteBytes(filePath, ToByteArray());
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
            }
            return true;
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

        public byte[] ToByteArray()
        {
            byte[] obj = BytesPacker.JSONObj(this);
            byte[][] msmData = new byte[mMsms.Count][];
            for (int i = 0; i < msmData.Length; i++)
            {
                Measurement msm = mMsms[i];
                msmData[i] = msm.ToByteArray();
            }
            byte[] msms = BytesPacker.PackBytes(msmData);
            return BytesPacker.PackBytes(obj, msms);
        }

        public bool LoadFromArray(byte[] array)
        {
            byte[][] objData = BytesPacker.UnpackBytes(array);
            if (objData.Length != 2) return false;

            User obj = BytesPacker.LoadJSONObj<User>(objData[0]);
            Name = obj.Name;
            SName = obj.SName;
            FName = obj.FName;
            ID = obj.ID;
            BirthDate = obj.BirthDate;
            Comment = obj.Comment;

            mMsms.Clear();
            byte[][] msmsArray = BytesPacker.UnpackBytes(objData[1]);
            bool result = true;
            foreach (byte[] bytes in msmsArray)
            {
                Measurement newMsm = new Measurement();
                result = result && newMsm.LoadFromArray(bytes);
                AddMsm(newMsm);
            }
            IsNotSaveChanges = false;
            return result;
        }

        #endregion

        #region No References
        public bool Save()
        {
            string filepath = string.IsNullOrEmpty(FilePath) ? DefaultPath(this) : FilePath;
            return Save(filepath);
        }

        public void RemoveMsm(Guid msmId)
        {
            Measurement msm = GetMsm(msmId);
            RemoveMsm(msm);
        }

        public Measurement GetMsm(int index)
        {
            if (mMsms.Count > index)
                return mMsms[index];
            return null;
        }

        public int GetMsmCount() => mMsms.Count;

        public IEnumerable<Measurement> GetMsms() => mMsms;

        public User(User user)
        {
            Init();
            LoadFromArray(user.ToByteArray());


            /*var pinfo = user.GetType().GetProperties();
            pinfo.ToList().ForEach( info => {
                if( info.CanRead && info.CanWrite ) {
                    info.SetValue( this, info.GetValue( user, null ), null );
                }
            } );
            mMsms = user.mMsms;*/
        }


        public string BirthDateString
        {
            get
            {
                string date = BirthDate.ToString("dd");
                string month = BirthDate.ToString("MMMM", new CultureInfo("ru-ru"));
                string year = BirthDate.ToString("yyyy");
                return date + "/" + month + "/" + year;
            }
        }
        #endregion
    }
}