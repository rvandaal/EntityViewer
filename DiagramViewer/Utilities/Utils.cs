using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace DiagramViewer.Utilities {
    public class Utils {

        public static double Halfsqrt2 = Math.Sqrt(2) / 2;
        public static double Halfsqrt3 = Math.Sqrt(3) / 2;
        public static Matrix Rotation45Matrix = new Matrix(Halfsqrt2, -Halfsqrt2, Halfsqrt2, Halfsqrt2, 0, 0);
        public static Matrix RotationMin45Matrix = new Matrix(Halfsqrt2, Halfsqrt2, -Halfsqrt2, Halfsqrt2, 0, 0);
        public static Matrix Rotation30Matrix = new Matrix(Halfsqrt3, -0.5, 0.5, Halfsqrt3, 0, 0);
        public static Matrix RotationMin30Matrix = new Matrix(Halfsqrt3, 0.5, -0.5, Halfsqrt3, 0, 0);

        public static readonly Pen DefaultPen = new Pen(new SolidColorBrush(Colors.Black), 1);
        public static readonly Brush DefaultBackgroundBrush = new SolidColorBrush(Color.FromRgb(221, 221, 221));
        public static readonly Brush DefaultForegroundBrush = new SolidColorBrush(Colors.Black);
        public static readonly SolidColorBrush NodeBrush = new SolidColorBrush(Colors.LightGoldenrodYellow);

        public static Vector Angle0Vector = new Vector(1, 0);
        public static Vector Angle90Vector = new Vector(0, 1);

        public static Typeface DefaultTypeface = new Typeface(
            new FontFamily("Arial"),
            new FontStyle(),
            new FontWeight(),
            new FontStretch()
        );

        public static FormattedText GetFormattedText(string text) {
            return new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                DefaultTypeface,
                12,
                new SolidColorBrush(Colors.Black)
            );
        }
    }
}
