using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace tEngine.MVVM.Converters {
    public abstract class ConverterBase<T> : MarkupExtension, IValueConverter
        where T : class, new() {
        private static T Converter = null;

        protected ConverterBase() {}

        /// <summary>
        /// Must be implemented in inheritor.
        /// </summary>
        public abstract object Convert( object value, Type targetType, object parameter,
            CultureInfo culture );

        /// <summary>
        /// Override if needed.
        /// </summary>
        public virtual object ConvertBack( object value, Type targetType, object parameter,
            CultureInfo culture ) {
            throw new NotImplementedException();
        }

        public override object ProvideValue( IServiceProvider serviceProvider ) {
            return Converter ?? (Converter = new T());
        }
    }

    public abstract class ConverterBaseM<T> : MarkupExtension, IMultiValueConverter
        where T : class, new() {
        private static T Converter = null;

        /// <summary>
        /// Must be implemented in inheritor.
        /// </summary>
        public abstract object Convert( object[] values, Type targetType, object parameter,
            CultureInfo culture );

        /// <summary>
        /// Override if needed.
        /// </summary>
        public virtual object[] ConvertBack( object value, Type[] targetTypes, object parameter,
            CultureInfo culture ) {
            throw new NotImplementedException();
        }

        public override object ProvideValue( IServiceProvider serviceProvider ) {
            return Converter ?? (Converter = new T());
        }
    }
}