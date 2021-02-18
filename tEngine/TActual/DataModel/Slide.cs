using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using tEngine.DataModel;
using tEngine.Helpers;

namespace tEngine.TActual.DataModel
{
    [DataContract]
    public class Slide
    {
        public enum SlideGrade
        {
            Essential, // важный
            Inessential // нет
        }

        public TData Data
        {
            get { return mData; }
        }

        public BitmapImage ImageBig { get; set; }
        public BitmapImage ImageMedium { get; set; }
        public BitmapImage ImageSmall { get; set; }

        public bool IsBaseReady
        {
            get { return mData.IsBaseData; }
        }

        public bool IsSpectrumReady
        {
            get { return mData.IsSpectrum; }
        }

        public Slide(Slide slide)
        {
            Id = slide.Id;
            RareFactor_Left = slide.RareFactor_Left;
            RareFactor_Right = slide.RareFactor_Right;
            Index = slide.Index;
            Grade = slide.Grade;
            Name = slide.Name;
            IsShow = slide.IsShow;
            FileUri = slide.FileUri;
            mData = slide.Data;
            ImageBig = slide.ImageBig.CloneCurrentValue();
            ImageMedium = slide.ImageMedium.CloneCurrentValue();
            ImageSmall = slide.ImageSmall.CloneCurrentValue();
        }

        public Slide()
        {
            Id = Guid.NewGuid();
            Grade = SlideGrade.Inessential;
            RareFactor_Left = new double[3]; // todo убрать константы
            RareFactor_Right = new double[3];
            IsShow = true;
        }

        public byte[] ToArray()
        {
            string json = JsonConvert.SerializeObject(this,
                new JsonSerializerSettings { ContractResolver = new JSONContractResolver() });
            byte[] bt1 = Encoding.Unicode.GetBytes(json);
            return bt1;
        }

        public bool UriLoad() => UriLoad(FileUri);

        public bool UriLoad(Uri uri)
        {
            try
            {
                // todo при кириллице может возникнуть переполнение пути, из-за змены символов
                ImageSmall = ImageHelper.Uri2BI(uri, ImageGrade.Small);
                ImageMedium = ImageHelper.Uri2BI(uri, ImageGrade.Medium);
                ImageBig = ImageHelper.Uri2BI(uri, ImageGrade.Big);
                //ImageTrue = ImageHelper.Uri2BI( uri );

                Id = Guid.NewGuid();
                Name = new FileInfo(uri.OriginalString).Name;
                FileUri = uri;
                //ImageByte = ImageHelper.BI2Array( ImageTrue );
                return true;
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                return false;
            }
        }

        private struct ImageGrade
        {
            public static readonly Size Big;
            public static readonly Size Medium;
            public static readonly Size Small;

            static ImageGrade()
            {
                Small = new Size(100, 100);
                Medium = new Size(500, 500);
                Big = new Size(SystemParameters.FullPrimaryScreenWidth, SystemParameters.FullPrimaryScreenHeight);
            }
        }

        #region Byte <=> Object

        public byte[] ToByteArray()
        {
            byte[] obj = BytesPacker.JSONObj(this);
            byte[] data = mData.ToByteArray();
            return BytesPacker.PackBytes(obj, data);
        }

        public bool LoadFromArray(byte[] array)
        {
            byte[][] objData = BytesPacker.UnpackBytes(array);
            if (objData.Length != 2) return false;

            Slide obj = BytesPacker.LoadJSONObj<Slide>(objData[0]);
            Id = obj.Id;
            RareFactor_Left = obj.RareFactor_Left;
            RareFactor_Right = obj.RareFactor_Right;
            Index = obj.Index;
            Grade = obj.Grade;
            Name = obj.Name;
            IsShow = obj.IsShow;
            FileUri = obj.FileUri;

            bool result = mData.LoadFromArray(objData[1]);
            return result;
        }

        #endregion


        public double RareFactor_Left_Summary
        {
            get { return RareFactor_Left.Average(); }
        }
        public double RareFactor_Right_Summary
        {
            get { return RareFactor_Left.Average(); }
        }

        #region JSON

        [DataMember]
        public Uri FileUri { get; set; }

        [DataMember(Name = "ID")]
        public Guid Id { get; private set; }

        private TData mData = new TData();
        public const string FOLDER_KEY = "LastNewImageFolder";

        //TODO: Переделать коэффициент необычности по человечески
        /// <summary>
        /// [тремор, спектр, корреляция]
        /// </summary>
        [DataMember(Name = "RF_Left")]
        public double[] RareFactor_Left { get; set; }
        /// <summary>
        /// [тремор, спектр, корреляция]
        /// </summary>
        [DataMember(Name = "RF_Right")]
        public double[] RareFactor_Right { get; set; }

        [DataMember]
        public int Index { get; set; }

        [DataMember]
        public SlideGrade Grade { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsShow { get; set; }

        #endregion
    }
}