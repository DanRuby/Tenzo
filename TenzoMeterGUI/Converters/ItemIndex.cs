using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace TenzoMeterGUI.Converters {
    internal class ItemIndex : IMultiValueConverter {
        /// <summary>
        /// Порядковый номер объекта в коллекции
        /// </summary>
        /// <param name="values">0 - коллекция, 1 - объект</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture ) {
            if( values != null ) {
                if( values.Count() == 2 ) {
                    var list = values[0] as IEnumerable<object>;
                    var obj = values[1];
                    if( list != null ) {
                        var i = list.ToList().IndexOf( obj );
                        if( i != -1 )
                            return i;
                    }
                }
            }
            return 0;
        }

        public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture ) {
            // Не требуется
            return null;
        }
    }
}