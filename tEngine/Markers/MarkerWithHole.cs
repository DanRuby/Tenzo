using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace tEngine.Markers
{
    public class MarkerWithHole : Marker
    {
        public int? Hole { get; set; }


        public MarkerWithHole()
        {
            Color = Colors.Red;
            Width = 40;
            Height = 6;
        }

        public override void Draw(WriteableBitmap bitmap, int yPos)
        {
            int delta = Hole ?? mHeight;
            double y1 = yPos - (delta + mHeight) / 2.0;
            double y2 = yPos + (delta + mHeight) / 2.0;

            base.Draw(bitmap, (int)y1);
            base.Draw(bitmap, (int)y2);
        }
    }
}