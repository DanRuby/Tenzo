using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using OxyPlot;

namespace tEngine.MVVM.Converters {
    public class PlotModelToBitmap : IValueConverter {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">PlotModel</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
            try {
                var pm = value as PlotModel;
                return GetBitmapFromPM( pm ) ?? Binding.DoNothing;
            } catch( Exception ex ) {
                if( new Observed<object>().IsDesignMode == false )
                    Debug.Assert( ex == null, ex.Message );
                return Binding.DoNothing;
            }
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
            return Binding.DoNothing;
        }

        public static BitmapSource GetBitmapFromPM( PlotModel pm ) {
            if( pm == null ) return null;
            pm.InvalidatePlot( false );
            if( pm.Width == 0 || pm.Height == 0 )
                return null;
            var pngExporter = new OxyPlot.Wpf.PngExporter();
            pngExporter.Background = OxyColors.Transparent;
            pngExporter.Height = (int) pm.Height;
            pngExporter.Width = (int) pm.Width;
            //pngExporter.Resolution = 95;
            return pngExporter.ExportToBitmap( pm );
        }
    }
}