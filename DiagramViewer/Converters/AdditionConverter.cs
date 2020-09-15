using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DiagramViewer.Converters {

    [ValueConversion(typeof(int), typeof(int))]
    internal sealed class AdditionConverter : IValueConverter {

        #region IValueConverter implementation

        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) {
            object result = DependencyProperty.UnsetValue;
            if ((value is double) && (parameter is double)) {
                result = (double)value + (double)parameter;
            }
            return result;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) {
            throw new NotImplementedException();
        }

        #endregion
    }
}