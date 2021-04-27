using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using tEngine.Helpers;

namespace tEngine.Markers
{
    /// <summary>
    /// Инкапсуляция маркера левой руки
    /// </summary>
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
            get => mColor;
            set
            {
                mColor = value;
                UpdateSource();
            }
        }

        public int Height
        {
            get => mHeight;
            set
            {
                mHeight = value;
                UpdateSource();
            }
        }

        public int Width
        {
            get => mWidth;
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

        /// <summary>
        /// Оновляеет битмап, используя свойства маркера 
        /// </summary>
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

        /// <summary>
        /// Нарисовать маркер в битмап
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="yPos">Верхняя граница маркера</param>
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