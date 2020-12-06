using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace TenzoMeterGUI.Converters {
    public class BooleanToVisibilityConverter : IValueConverter {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
            var bl = value as bool?;
            if( parameter is string && parameter.Equals( "not" ) )
                bl = !bl;
            return bl == true ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
            var bl = value as Visibility?;
            if( parameter is string && parameter.Equals( "not" ) )
                return bl != Visibility.Visible;
            return bl == Visibility.Visible;
        }
    }
}