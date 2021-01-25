using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using tEngine.DataModel;
using tEngine.Helpers;

namespace tEngine.TMeter.DataModel.RschUnit
{
    /// <summary>
    /// Важность слайда
    /// </summary>
    public enum SlideGrade
    {
        Essential,
        Inessential
    }

    [DataContract]
    public class Slide
    {
        private readonly Size mBigSize = new Size(SystemParameters.FullPrimaryScreenWidth,
            SystemParameters.FullPrimaryScreenHeight);

        private readonly Size mSmallSize = new Size(200, 200);
        private string mComment;
        private TData mData = new TData();
        private string mFilePath;
        private SlideGrade mGrade;
        private Guid mId;
        private BitmapImage mImageBig = new BitmapImage();
        private BitmapImage mImageSmall = new BitmapImage();
        private int mIndex = 0;
        private double mRate;

        public string Comment
        {
            get { return mComment; }
            set { mComment = value; }
        }

        public string FilePath
        {
            get { return mFilePath; }
            set { mFilePath = value; }
        }

        public SlideGrade Grade
        {
            get { return mGrade; }
            set { mGrade = value; }
        }

        public Guid Id
        {
            get { return mId; }
            set { mId = value; }
        }

        [XmlIgnore]
        public BitmapImage ImageBig
        {
            get { return mImageBig; }
            private set { mImageBig = value; }
        }

        [XmlIgnore]
        public BitmapImage ImageSmall
        {
            get { return mImageSmall; }
            private set { mImageSmall = value; }
        }

        public int Index
        {
            get { return mIndex; }
            set { mIndex = value; }
        }

        public double Rate
        {
            get { return mRate; }
            set { mRate = value; }
        }

        public Slide()
        {
            mId = Guid.NewGuid();
            mGrade = SlideGrade.Inessential;
            mRate = 0;
        }

        public bool SetImage(string filepath)
        {
            mFilePath = filepath;
            try
            {
                ImageBig = GetSimilarImage(filepath, mBigSize);
                ImageSmall = GetSimilarImage(filepath, mSmallSize);
                return true;
            }
            catch (Exception ex)
            {
                Logger.ShowException(ex);
                return false;
            }
        }

        private BitmapImage GetSimilarImage(string filepath, Size size)
        {
            BitmapImage orignBi = new BitmapImage(new Uri(filepath, UriKind.RelativeOrAbsolute));
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(filepath, UriKind.RelativeOrAbsolute);
            double w = size.Width / orignBi.PixelWidth;
            double h = size.Height / orignBi.PixelHeight;
            double k = w > h ? h : w;
            bi.DecodePixelHeight = (int)(k * orignBi.PixelHeight);
            bi.DecodePixelWidth = (int)(k * orignBi.PixelWidth);
            bi.EndInit();
            return bi;
        }
    }
}