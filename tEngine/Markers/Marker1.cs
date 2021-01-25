using System.Runtime.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace tEngine.Markers
{
    [DataContract]
    public class Marker1 : Marker
    {
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

        public Marker1()
        {
            Color = Colors.Blue;
            Width = 60;
            Height = 6;
        }

        public new void Draw(WriteableBitmap bitmap, int yPos)
        {
            base.Draw(bitmap, yPos);
        }
    }
}