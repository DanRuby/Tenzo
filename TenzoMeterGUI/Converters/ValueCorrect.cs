using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using tEngine.MVVM.Converters;

namespace TenzoMeterGUI.Converters {
    public class ValueCorrect :ConverterBase<ValueCorrect> {
        public override object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
            var str = parameter as string;
            var offset = 0;
            if( int.TryParse( str, out offset ) ) {
                return (double) value + offset;
            }
            return Binding.DoNothing;
        }
    }
}
