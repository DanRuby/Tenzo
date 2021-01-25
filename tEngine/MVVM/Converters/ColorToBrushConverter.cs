using System;
using System.Globalization;
using System.Windows.Media;

namespace tEngine.MVVM.Converters
{
    public class ColorToBrushConverter : ConverterBase<ColorToBrushConverter> {
        public override object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
            var color = value as Color? ?? Colors.White;
            var brush = new SolidColorBrush( color );
            return brush;
        }

        public ColorToBrushConverter() {}

        public override object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
            var brush = value as SolidColorBrush ?? new SolidColorBrush( Colors.White );
            var color = brush.Color;
            return color;
        }
    }
}