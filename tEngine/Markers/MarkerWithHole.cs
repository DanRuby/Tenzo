using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace tEngine.Markers
{
    /// <summary>
    /// Инкапсуляция маркера правой руки
    /// </summary>
    public class MarkerWithHole : Marker
    {
        /// <summary>
        /// Промежуток между двумя частями маркера 
        /// </summary>
        public int? Hole { get; set; }


        public MarkerWithHole()
        {
            Color = Colors.Red;
            Width = 40;
            Height = 6;
        }

        /// <summary>
        /// Рисует маркер на битмапе
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="yPos">Центр маркера</param>
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