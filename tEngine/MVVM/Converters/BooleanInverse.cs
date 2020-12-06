using System;
using System.Globalization;
using System.Windows.Data;

namespace tEngine.MVVM.Converters {
    public class BooleanInverse : ConverterBase<BooleanInverse> {
        public BooleanInverse() {}

        public override object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
            var bl = value as bool? ?? true;
            return !bl;
        }

        public override object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
            var bl = value as bool? ?? false;
            return !bl;
        }
    }
}