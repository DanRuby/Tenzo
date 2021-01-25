using System;
using System.Globalization;
using tEngine.MVVM.Converters;

namespace TenzoMeterGUI.Converters
{
    public class BooleanToSign : ConverterBase<BooleanToSign>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value as bool?) == true) return "+";
            return "-";
        }
    }
}
