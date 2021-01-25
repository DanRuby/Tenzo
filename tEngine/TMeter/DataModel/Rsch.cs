using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using tEngine.Helpers;
using tEngine.TMeter.DataModel.IData;

namespace tEngine.TMeter.DataModel
{
    [DataContract]
    public class Rsch : IRsch<Rsch>
    {
        private readonly Func<Rsch, string> mDefaultFilePath = (r) =>
        {
            var cDirectory = AppSettings.GetValue("LastUserFolder", Constants.AppDataFolder);
            if (!cDirectory.EndsWith(@"\") && !cDirectory.EndsWith("/"))
                cDirectory += @"\";
            var filepath = cDirectory;
            filepath += r.Title + Constants.USER_EXT;
            return filepath;
        };

        private List<Measurement> mMsms = new List<Measurement>();
        public string Comment { get; set; }
        public DateTime CreateTime { get; set; }
        public string FilePath { get; set; }
        public Guid ID { get; private set; }
        public string Title { get; set; }

        Guid IRsch<Rsch>.ID
        {
            get { return this.ID; }
            set { this.ID = value; }
        }

        private List<Guid> MsmsGuids { get; set; }

        List<Guid> IRsch<Rsch>.MsmsGuids
        {
            get { return this.MsmsGuids; }
            set { this.MsmsGuids = value; }
        }

        private List<string> UsersPaths { get; set; }

        List<string> IRsch<Rsch>.UsersPaths
        {
            get { return this.UsersPaths; }
            set { this.UsersPaths = value; }
        }

        public Rsch()
        {
            ID = Guid.NewGuid();
            MsmsGuids = new List<Guid>();
            UsersPaths = new List<string>();
            FilePath = mDefaultFilePath(this);
        }

        public void AddMsm(User user, Guid msmId)
        {
            if (GetMsm(msmId) == null)
                mMsms.Add(user.GetMsm(msmId));
            UpdatePaths();
        }

        public Measurement GetMsm(Guid msmId)
        {
            var msms = mMsms.Where(msm => msm.ID.Equals(msmId));
            var enumerable = msms as Measurement[] ?? msms.ToArray();
            return enumerable.Any() ? enumerable[0] : null;
        }

        public int GetMsmCount() => MsmsGuids.Count;

        public IEnumerable<Measurement> GetMsms() => mMsms;

        public IEnumerable<User> GetUsers()
        {
            var users = mMsms.Select(msm => msm.GetOwner());
            users = users.Distinct();
            return users;
        }

        public int GetUsersCount() => UsersPaths.Count;

        public bool Open(string filePath, out Rsch rsch)
        {
            try
            {
                string json;
                var result = FileIO.ReadText(filePath, out json);
                rsch = JsonConvert.DeserializeObject<Rsch>(json);
                rsch.FilePath = filePath;
                return result;
            }
            catch (Exception)
            {
                rsch = new Rsch();
                return false;
            }
        }

        public void RemoveMsm(Guid msmId) => mMsms.Remove(GetMsm(msmId));

        public void RemoveMsm(Measurement msm)
        {
            mMsms.Remove(msm);
            UpdatePaths();
        }

        public bool Save()
        {
            var filepath = string.IsNullOrEmpty(FilePath) ? mDefaultFilePath(this) : FilePath;
            return Save(filepath);
        }

        public bool Save(string filePath)
        {
            UpdatePaths();
            var settings = new JsonSerializerSettings() { ContractResolver = new JSONContractResolver() };
            var json = JsonConvert.SerializeObject(this, settings);
            FileIO.WriteText(filePath, json);
            return true;
        }

        /// <summary>
        /// Обновление путей к файлам пациентов и ID измерений
        /// </summary>
        private void UpdatePaths()
        {
            MsmsGuids.Clear();
            mMsms.ForEach(msm => MsmsGuids.Add(msm.ID));
            UsersPaths.Clear();
            GetUsers().ToList().ForEach(user => UsersPaths.Add(user.FilePath));
        }
    }
}