using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace tEngine.Markers
{
    public class Marker2 : Marker
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

        public int? Hole { get; set; }

        public int Width
        {
            get { return mWidth; }
            set
            {
                mWidth = value;
                UpdateSource();
            }
        }

        public Marker2()
        {
            Color = Colors.Red;
            Width = 40;
            Height = 6;
        }

        public new void Draw(WriteableBitmap bitmap, int yPos)
        {
            int delta = Hole ?? mHeight;
            double y1 = yPos - (delta + mHeight) / 2.0;
            double y2 = yPos + (delta + mHeight) / 2.0;

            base.Draw(bitmap, (int)y1);
            base.Draw(bitmap, (int)y2);
        }
    }
}