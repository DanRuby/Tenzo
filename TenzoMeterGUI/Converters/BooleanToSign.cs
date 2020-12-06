using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using tEngine.MVVM.Converters;

namespace TenzoMeterGUI.Converters {
    public class BooleanToSign:ConverterBase<BooleanToSign> {
        public override object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
            if( (value as bool?) == true ) return "+";
            return "-";
        }
    }
}
