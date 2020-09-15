using System.Windows;
using System.Windows.Media;

namespace GraphFramework {
    public class Utils {
        public static readonly Pen DefaultPen = new Pen(new SolidColorBrush(Colors.Black), 1);
        public static readonly SolidColorBrush NodeBrush = new SolidColorBrush(Colors.LightGoldenrodYellow);

        public static Vector Angle0Vector = new Vector(1, 0);
        public static Vector Angle90Vector = new Vector(0, 1);
    }
}
