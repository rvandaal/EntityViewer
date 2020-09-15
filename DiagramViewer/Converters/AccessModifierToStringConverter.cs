using System;
using System.Windows.Data;
using DiagramViewer.Models;

namespace DiagramViewer.Converters {
    public class AccessModifierToStringConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if(value is AccessModifier) {
                switch ((AccessModifier)value) {
                    case AccessModifier.Private:
                        return "-";
                    case AccessModifier.Protected:
                        return "#";
                    case AccessModifier.ProtectedInternal:
                        return "$#";
                    case AccessModifier.Internal:
                        return "$";
                    default:
                        return "";
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
