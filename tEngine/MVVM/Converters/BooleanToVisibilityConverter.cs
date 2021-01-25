using System;
using System.Globalization;
using System.Windows;

namespace tEngine.MVVM.Converters
{
    public class BooleanToVisibilityConverter : ConverterBase<BooleanToVisibilityConverter>
    {
        public BooleanToVisibilityConverter() { }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? bl = (value as bool?);
            string param = parameter as string;
            if (param != null && param.ToLower().Equals("inverse"))
            {
                bl = (bl == null || bl == false);
            }
            switch (bl)
            {
                case null:
                    return Visibility.Collapsed;
                case true:
                    return Visibility.Visible;
                case false:
                    return Visibility.Hidden;
            }
            return Visibility.Visible;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility? vb = value as Visibility?;
            string param = parameter as string;
            if (param != null && param.ToLower().Equals("inverse"))
            {
                return vb != Visibility.Visible;
            }
            return vb == Visibility.Visible;
        }
    }
}