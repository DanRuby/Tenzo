using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace TenzoMeterGUI.Converters {
    public class StringCalculate : IValueConverter {
        /// <summary>
        /// Делает математическую операцию над значением
        /// </summary>
        /// <param name="value">число</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">строка вид "{0}*2", "{0} + 4" и т.д. </param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
            throw new NotImplementedException();
            //var str = parameter as string ?? "";
            //str = string.Format( str, value );
            //var result = (double) (new Expression( str )).Evaluate();
            //return result < 0 ? 0 : result;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
            throw new NotImplementedException();
            //return null;
        }
    }
}