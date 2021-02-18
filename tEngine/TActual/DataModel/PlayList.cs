using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using tEngine.Helpers;

namespace tEngine.TActual.DataModel
{
    [DataContract]
    public class PlayList
    {
        public enum MoveDirection
        {
            Up,
            Down
        }

        public enum SortBy
        {
            Index,
            RareFactor,
        }

        public const string FOLDER_KEY = "LastPlayListFolder";
        private bool mIsNotSaveChanges;
        public string FilePath { get; set; }

        public bool IsNotSaveChanges
        {
            get { return mIsNotSaveChanges; }
            set { mIsNotSaveChanges = value; }
        }

        public ReadOnlyCollection<Slide> Slides { get; set; }

        public PlayList(PlayList playList)
        {
            List<Slide> list = new List<Slide>();
            list.AddRange(playList.Slides.Select(slide => new Slide(slide)));

            Slides = new ReadOnlyCollection<Slide>(list);
            ID = playList.ID;
            CreateTime = playList.CreateTime;
            SecondsToSlide = playList.SecondsToSlide;
            Title = playList.Title;
        }

        public PlayList()
        {
            ID = Guid.NewGuid();
            SecondsToSlide = 5;
            CreateTime = DateTime.Now;
            Slides = new ReadOnlyCollection<Slide>(new List<Slide>());
            FilePath = DefaultPath(this);
            IsNotSaveChanges = false;
        }

        public void AddRangeSlide(IEnumerable<Slide> slides)
        {
            int index = Slides.Count() + 1;
            List<Slide> list = Slides.ToList();
            list.AddRange(slides.Select(slide =>
            {
                slide.Index = index;
                return slide;
            }));
            Slides = new ReadOnlyCollection<Slide>(list);
            Sort();
            IsNotSaveChanges = true;
        }

        public void AddSlide(Slide slide)
        {
            slide.Index = Slides.Count() + 1;
            List<Slide> list = Slides.ToList();
            list.Add(slide);
            Slides = new ReadOnlyCollection<Slide>(list);
            Sort();
            IsNotSaveChanges = true;
        }

        public void MoveSlide(Slide slide, MoveDirection dir)
        {
            List<Slide> list = Slides.ToList();
            int indexOld = list.IndexOf(slide);
            int indexNew = dir == MoveDirection.Up ? indexOld - 1 : indexOld + 1;
            if (Math.Min(indexOld, indexNew) >= 0 && Math.Max(indexOld, indexNew) < list.Count)
            {
                list[indexOld].Index = indexNew;
                list[indexNew].Index = indexOld;
            }
            Sort();
            IsNotSaveChanges = true;
        }

        public static bool Open(string filePath, out PlayList playList)
        {
            try
            {
                string json;
                bool result = FileIO.ReadText(filePath, out json);
                playList = JsonConvert.DeserializeObject<PlayList>(json);
                playList.FilePath = filePath;
                DirectoryInfo folder = new FileInfo(playList.FilePath).Directory;
                if (folder != null)
                    AppSettings.SetValue(PlayList.FOLDER_KEY, folder.FullName);
                if (playList.Slides == null) playList.Slides = new ReadOnlyCollection<Slide>(new List<Slide>());
                foreach (Slide slide in playList.Slides)
                {
                    slide.UriLoad();
                }
                playList.Sort();

                playList.IsNotSaveChanges = false;
                return result;
            }
            catch (Exception)
            {
                playList = new PlayList();
                return false;
            }
        }

        private double GetCorrelation(IEnumerable<double> data1, IEnumerable<double> data2)
        {
            if (data1 == null || !data1.Any() || data2 == null || !data2.Any())
                return 0;
            int size = data1.Count();
            if (data2.Count() < size)
            {
                size = data2.Count();
            }
            return MathNet.Numerics.Statistics.Correlation.Pearson(data1.Take(size), data2.Take(size));
        }

        private void GetCorrelationMatrix(ref double[,,] matrix, double[][][] dataArray, int count)
        {
            for (int i = 0; i < count; i++)
            {
                double[] tremor1 = dataArray[i][0];
                double[] spectrum1 = dataArray[i][1];
                double[] corr1 = dataArray[i][2];
                for (int j = i; j < count; j++)
                {

                    double[] tremor2 = dataArray[j][0];
                    double[] spectrum2 = dataArray[j][1];
                    double[] corr2 = dataArray[j][2];

                    matrix[i, j, 0] = (GetCorrelation(tremor1, tremor2) + 1) / 2;
                    matrix[i, j, 1] = (GetCorrelation(spectrum1, spectrum2) + 1) / 2;
                    matrix[i, j, 2] = (GetCorrelation(corr1, corr2) + 1) / 2;
                    matrix[j, i, 0] = matrix[i, j, 0];
                    matrix[j, i, 1] = matrix[i, j, 1];
                    matrix[j, i, 2] = matrix[i, j, 2];
                }
            }
        }

        public void RareCalculate()
        {
            int count = Slides.Count;
            double[][][] leftData = new double[count][][];
            double[][][] rightData = new double[count][][];
            for (int i = 0; i < count; i++)
            {
                tEngine.DataModel.HAND_DATA left = Slides[i].Data.Left;
                tEngine.DataModel.HAND_DATA right = Slides[i].Data.Right;

                leftData[i] = new double[3][];
                rightData[i] = new double[3][];

                leftData[i][0] = left.IsTremor ? left.Tremor.Data.Select(dp => dp.Y).ToArray() : null;
                leftData[i][1] = left.IsSpectrum ? left.Spectrum.Data.Select(dp => dp.Y).ToArray() : null;
                leftData[i][2] = left.IsCorrelation ? left.Spectrum.Data.Select(dp => dp.Y).ToArray() : null;

                rightData[i][0] = right.IsTremor ? right.Tremor.Data.Select(dp => dp.Y).ToArray() : null;
                rightData[i][1] = right.IsSpectrum ? right.Spectrum.Data.Select(dp => dp.Y).ToArray() : null;
                rightData[i][2] = right.IsCorrelation ? right.Spectrum.Data.Select(dp => dp.Y).ToArray() : null;
            }

            // левая рука
            double[,,] rareLeft = new double[count, count, 3];
            GetCorrelationMatrix(ref rareLeft, leftData, count);

            // правая рука
            double[,,] rareRight = new double[count, count, 3];
            GetCorrelationMatrix(ref rareRight, rightData, count);

            for (int i = 0; i < count; i++)
            {
                double[] slideRareLeft = new double[3] { 0, 0, 0 };
                double[] slideRareRight = new double[3] { 0, 0, 0 };
                for (int j = 0; j < count; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        slideRareLeft[k] += rareLeft[i, j, k];
                        slideRareRight[k] += rareRight[i, j, k];
                    }
                }
                Slides[i].RareFactor_Left = slideRareLeft.ToArray();
                Slides[i].RareFactor_Right = slideRareRight.ToArray();
            }
            IsNotSaveChanges = true;
        }

        public int RemoveSlide(Slide slide)
        {
            List<Slide> list = Slides.ToList();
            int index = list.IndexOf(slide);
            list.Remove(slide);
            Slides = new ReadOnlyCollection<Slide>(list);
            Sort();

            while (index >= list.Count)
                index--;
            IsNotSaveChanges = true;
            return index;
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
                DirectoryInfo dinfo = new FileInfo(filePath).Directory;
                if (dinfo != null)
                {
                    if (!dinfo.Exists) dinfo.Create();

                    JsonSerializerSettings settings = new JsonSerializerSettings { ContractResolver = new JSONContractResolver() };
                    string json = JsonConvert.SerializeObject(this, settings);
                    FileIO.WriteText(filePath, json);

                    IsNotSaveChanges = false;
                }
            }
            catch (Exception)
            {
            }
            return true;
        }
        public void MixSlides()
        {
            List<Slide> list = Slides.OrderBy(slide => Guid.NewGuid()).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Index = i;
            }
            Sort();
            IsNotSaveChanges = true;
        }

        public void Sort(SortBy key = SortBy.Index)
        {
            IEnumerable<Slide> list = new List<Slide>();
            if (key == SortBy.Index)
            {
                list = Slides.OrderBy(slide => slide.Index);
            }
            else if (key == SortBy.RareFactor)
            {
                //list = Slides.OrderBy( slide => slide.RareFactor_Left );
            }
            list = list.Select((slide, i) =>
            {
                slide.Index = i + 1;
                return slide;
            });
            Slides = new ReadOnlyCollection<Slide>(list.ToList());
            IsNotSaveChanges = true;
        }

        private string DefaultPath(PlayList playList)
        {
            string cDirectory = AppSettings.GetValue(PlayList.FOLDER_KEY, Constants.AppDataFolder);
            string filepath = cDirectory.CorrectSlash();
            filepath += playList.Title + Constants.PlayListExt;
            return filepath;
        }

        #region JSON

        [DataMember]
        public DateTime CreateTime { get; set; }

        [DataMember]
        public Guid ID { get; private set; }

        [DataMember]
        public double SecondsToSlide { get; set; }

        [DataMember]
        public string Title { get; set; }

        #endregion

        #region Byte <=> Object

        public byte[] ToByteArray()
        {
            byte[] obj = BytesPacker.JSONObj(this);
            byte[][] slidesData = new byte[Slides.Count][];
            for (int i = 0; i < Slides.Count; i++)
            {
                Slide slide = Slides[i];
                slidesData[i] = slide.ToByteArray();
            }
            byte[] slides = BytesPacker.PackBytes(slidesData);

            return BytesPacker.PackBytes(obj, slides);
        }

        public bool LoadFromArray(byte[] array)
        {
            byte[][] objData = BytesPacker.UnpackBytes(array);
            if (objData.Length != 2) return false;

            PlayList obj = BytesPacker.LoadJSONObj<PlayList>(objData[0]);
            ID = obj.ID;
            CreateTime = obj.CreateTime;
            SecondsToSlide = obj.SecondsToSlide;
            Title = obj.Title;

            byte[][] slidesArray = BytesPacker.UnpackBytes(objData[1]);
            bool result = true;
            foreach (byte[] bytes in slidesArray)
            {
                Slide newSlide = new Slide();
                result = result && newSlide.LoadFromArray(bytes);
                AddSlide(newSlide);
            }
            IsNotSaveChanges = false;
            return result;
        }

        #endregion


    }
}