using System;
using System.Globalization;
using System.Windows.Data;
using tEngine.MVVM.Converters;

namespace TenzoMeterGUI.Converters
{
    public class ValueCorrect : ConverterBase<ValueCorrect>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = parameter as string;
            int offset = 0;
            if (int.TryParse(str, out offset))
            {
                return (double)value + offset;
            }
            return Binding.DoNothing;
        }
    }
}
