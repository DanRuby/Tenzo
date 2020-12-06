using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using tEngine.MVVM.Converters;

namespace TenzoMeterGUI.Converters {
    public class ListIndexer : ConverterBaseM<ListIndexer> {
        public override object Convert( object[] values, Type targetType, object parameter, CultureInfo culture ) {
            if( (values[1] as bool?) == true ) {
                if( targetType == typeof( int ) ) {
                    // запрос по индексу, пришел от SelectedIndex
                    return values[0];
                }
                return Binding.DoNothing;
            } else {
                if( targetType == typeof( int ) ) {
                    return Binding.DoNothing;
                }
                return values[0];
            }
        }

        public override object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture ) {
            return new object[] {value, false};
        }
    }
}