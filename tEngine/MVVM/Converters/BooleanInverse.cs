using System;
using System.Globalization;

namespace tEngine.MVVM.Converters
{
    public class BooleanInverse : ConverterBase<BooleanInverse>
    {
        public BooleanInverse() { }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bl = value as bool? ?? true;
            return !bl;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bl = value as bool? ?? false;
            return !bl;
        }
    }
}