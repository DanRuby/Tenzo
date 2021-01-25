using System;
using System.Globalization;
using System.Windows.Media;

namespace tEngine.MVVM.Converters
{
    public class ColorToBrushConverter : ConverterBase<ColorToBrushConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color color = value as Color? ?? Colors.White;
            SolidColorBrush brush = new SolidColorBrush(color);
            return brush;
        }

        public ColorToBrushConverter() { }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush brush = value as SolidColorBrush ?? new SolidColorBrush(Colors.White);
            Color color = brush.Color;
            return color;
        }
    }
}