using System;
using System.Globalization;
using System.Windows.Data;
using NCalc;

namespace tEngine.MVVM.Converters
{
    public class StringCalculate : IValueConverter
    {
        /// <summary>
        /// Делает математическую операцию над значением
        /// </summary>
        /// <param name="value">число</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">строка вид "{0}*2", "{0} + 4" и т.д. </param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = parameter as string ?? "";
            str = string.Format(str, value);
            double result = (double)(new Expression(str)).Evaluate();
            return result < 0 ? 0 : result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}