using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using tEngine.MVVM.Converters;

namespace TenzoActualGUI.Helpers
{
    public class ArrayToAverage:ConverterBase<ArrayToAverage> {
        public override object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
            var canCalc = (value is IEnumerable<double>);
            Debug.Assert( canCalc == true );
            if( canCalc ) 
                return ((IEnumerable<double>) value).Average();
            
            return Binding.DoNothing;
        }
    }
}
