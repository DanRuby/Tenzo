using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using tEngine.TActual.DataModel;

namespace tEngine.MVVM.Converters {
    public class GradeToBoolean : IValueConverter {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
            return value as Slide.SlideGrade? == Slide.SlideGrade.Essential;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
            return (value as bool? == true) ? Slide.SlideGrade.Essential : Slide.SlideGrade.Inessential;
        }
    }
}