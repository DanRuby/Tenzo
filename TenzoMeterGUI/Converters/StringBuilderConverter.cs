using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace TenzoMeterGUI.Converters {
    public class StringBuilderConverter : IMultiValueConverter {
        public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture ) {
            var str = parameter as string ?? "";
            return string.Format( str, values );
        }

        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture ) {
            // не требуется
            return null;
        }
    }
}