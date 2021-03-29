using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using tEngine.Helpers;

namespace tEngine.Markers
{
    [DataContract]
    public class Marker
    {
        [DataMember(Name = "bc")]
        protected Color mColor;

        [DataMember(Name = "bh")]
        protected int mHeight;

        protected WriteableBitmap mSource;

        [DataMember(Name = "bw")]
        protected int mWidth;

        public Color Color
        {
            get { return mColor; }
            set
            {
                mColor = value;
                UpdateSource();
            }
        }

        public int Height
        {
            get { return mHeight; }
            set
            {
                mHeight = value;
                UpdateSource();
            }
        }

        public int Width
        {
            get { return mWidth; }
            set
            {
                mWidth = value;
                UpdateSource();
            }
        }

        public Marker()
        {
            UpdateSource();
        }

        public void UpdateSource()
        {
            if (mWidth == 0 || mHeight == 0)
            {
                mSource = null;
                return;
            }
            mSource = new WriteableBitmap(mWidth, mHeight, 96d, 96d, PixelFormats.Bgr24, null);
            Drawer.DrawRectangle(mSource, new Rect(0, 0, mSource.PixelWidth, mSource.PixelHeight), mColor);
        }

        public virtual void Draw(WriteableBitmap bitmap, int yPos)
        {
            if (bitmap == null)
            {
                Logger.ShowException(new Exception("no area container"));
                return;
            }
            if (mSource == null)
            {
                Logger.ShowException(new Exception("no marker source"));
                return;
            }
            double left = (bitmap.Width - mWidth) / 2;
            double top = yPos - mHeight / 2.0;
            Drawer.CopyPart(mSource, new Rect(0, 0, mWidth, mHeight), bitmap, new Rect(left, top, mWidth, mHeight));
        }
    }
}