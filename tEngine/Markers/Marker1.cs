using System.Runtime.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace tEngine.Markers
{
    [DataContract]
    public class Marker1 : Marker
    {
        public Marker1()
        {
            Color = Colors.Blue;
            Width = 60;
            Height = 6;
        }
    }
}