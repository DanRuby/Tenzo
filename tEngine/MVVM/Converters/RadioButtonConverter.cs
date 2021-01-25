using System;
using System.Globalization;
using System.Windows.Data;

namespace tEngine.MVVM.Converters
{
    public class RadioButtonConverter : ConverterBase<RadioButtonConverter>
    {
        public RadioButtonConverter() { }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}