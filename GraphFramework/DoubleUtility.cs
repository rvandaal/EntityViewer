using System;
using System.Windows;

namespace GraphFramework {

    /// <summary>
    /// Utility class that contains helper methods to compare doubles.
    /// </summary>
    /// <remarks>
    /// TODO: This file is copied from Microsoft code and has to be checked regarding the license.
    /// The source code can be found in:
    /// <list type="bullet">
    ///   <item>
    ///     <term>Type:</term>
    ///     <description>MS.Internal.DoubleUtil</description>
    ///   </item>
    ///   <item>
    ///     <term>Assembly:</term>
    ///     <description>WindowsBase, Version=4.0.0.0, Culture=neutral,
    ///     PublicKeyToken=31bf3856ad364e35</description>
    ///   </item>
    /// </list>
    /// </remarks>
    public static class DoubleUtility {

        #region Internal constants

        /// <summary>
        /// The smallest number by which two doubles may differ and still be considered close enough
        /// to be equal; in other words, a measure for the precision of double calculus provided by
        /// this utility class.
        /// </summary>
        internal const double DoubleEpsilon = 2.22044604925031E-16;

        /// <summary>
        /// The minimum value for a floating point.
        /// </summary>
        internal const float MinimalFloat = 1.175494E-38f;

        #endregion

        #region Public methods

        /// <summary>
        /// Determines whether two doubles are close to each other.
        /// </summary>
        /// <param name="value1">The first double value to compare.</param>
        /// <param name="value2">The second double value to compare.</param>
        public static bool AreClose(double value1, double value2) {
            if (value1 == value2) {
                return true;
            }
            var num1 = (Math.Abs(value1) + Math.Abs(value2) + 10.0) * DoubleEpsilon;
            var num2 = value1 - value2;
            if (-num1 < num2) {
                return num1 > num2;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the first double is less than the second double.
        /// </summary>
        /// <param name="value1">The first double value to compare.</param>
        /// <param name="value2">The second double value to compare.</param>
        public static bool LessThan(double value1, double value2) {
            if (value1 < value2) {
                return !AreClose(value1, value2);
            }
            return false;
        }

        /// <summary>
        /// Determines whether the first double is greater than the second double.
        /// </summary>
        /// <param name="value1">The first double value to compare.</param>
        /// <param name="value2">The second double value to compare.</param>
        public static bool GreaterThan(double value1, double value2) {
            if (value1 > value2) {
                return !AreClose(value1, value2);
            }
            return false;
        }

        /// <summary>
        /// Determines whether the first double is less than the second double or close to it.
        /// </summary>
        /// <param name="value1">The first double value to compare.</param>
        /// <param name="value2">The second double value to compare.</param>
        public static bool LessThanOrClose(double value1, double value2) {
            if (value1 >= value2) {
                return AreClose(value1, value2);
            }
            return true;
        }

        /// <summary>
        /// Determines whether the first double is greater than the second double or close to it.
        /// </summary>
        /// <param name="value1">The first double value to compare.</param>
        /// <param name="value2">The second double value to compare.</param>
        public static bool GreaterThanOrClose(double value1, double value2) {
            if (value1 <= value2) {
                return AreClose(value1, value2);
            }
            return true;
        }

        /// <summary>
        /// Determines whether the double is close to one.
        /// </summary>
        /// <param name="value">
        /// The double value for which we want to determine whether it is close to one.
        /// </param>
        public static bool IsOne(double value) {
            return (Math.Abs(value - 1.0) < DoubleEpsilon);
        }

        /// <summary>
        /// Determines whether the double is close to zero.
        /// </summary>
        /// <param name="value">
        /// The double value for which we want to determine whether it is close to zero.
        /// </param>
        public static bool IsZero(double value) {
            return (Math.Abs(value) < DoubleEpsilon);
        }

        /// <summary>
        /// Determines whether two points are close to each other.
        /// </summary>
        /// <param name="point1">The first point to compare.</param>
        /// <param name="point2">The second point to compare.</param>
        public static bool AreClose(Point point1, Point point2) {
            if (AreClose(point1.X, point2.X)) {
                return AreClose(point1.Y, point2.Y);
            }
            return false;
        }

        /// <summary>
        /// Determines whether two sizes are close to each other.
        /// </summary>
        /// <param name="size1">The first size to compare.</param>
        /// <param name="size2">The second size to compare.</param>
        public static bool AreClose(Size size1, Size size2) {
            if (AreClose(size1.Width, size2.Width)) {
                return AreClose(size1.Height, size2.Height);
            }
            return false;
        }

        /// <summary>
        /// Determines whether two vectors are close to each other.
        /// </summary>
        /// <param name="vector1">The first vector to compare.</param>
        /// <param name="vector2">The second vector to compare.</param>
        public static bool AreClose(Vector vector1, Vector vector2) {
            if (AreClose(vector1.X, vector2.X)) {
                return AreClose(vector1.Y, vector2.Y);
            }
            return false;
        }

        /// <summary>
        /// Determines whether two rectangles are close to each other.
        /// </summary>
        /// <param name="rectangle1">The first rectangle to compare.</param>
        /// <param name="rectangle2">The second rectangle to compare.</param>
        public static bool AreClose(Rect rectangle1, Rect rectangle2) {
            if (rectangle1.IsEmpty) {
                return rectangle2.IsEmpty;
            }
            if (
                !rectangle2.IsEmpty &&
                AreClose(rectangle1.X, rectangle2.X) &&
                (AreClose(rectangle1.Y, rectangle2.Y) &&
                AreClose(rectangle1.Height, rectangle2.Height))
            ) {
                return AreClose(rectangle1.Width, rectangle2.Width);
            }
            return false;
        }

        /// <summary>
        /// Determines whether a double is between zero and one.
        /// </summary>
        /// <param name="value">
        /// The double value for which we want to determine whether it is between zero and one.
        /// </param>
        public static bool IsBetweenZeroAndOne(double value) {
            if (GreaterThanOrClose(value, 0.0)) {
                return LessThanOrClose(value, 1.0);
            }
            return false;
        }

        /// <summary>
        /// Converts a double into an integer.
        /// </summary>
        /// <param name="value">The double value to convert into an integer.</param>
        public static int DoubleToInt(double value) {
            if (0.0 >= value) {
                return (int)(value - 0.5);
            }
            return (int)(value + 0.5);
        }

        /// <summary>
        /// Determines whether a rectangle contains a position or size that is not a number.
        /// </summary>
        /// <param name="rectangle">
        /// The rectangle for which to determine whether it has a X-axis value, a Y-axis value,
        /// a height, or a width that is not a number.
        /// </param>
        public static bool RectHasNaN(Rect rectangle) {
            return (
                IsNaN(rectangle.X) ||
                IsNaN(rectangle.Y) ||
                IsNaN(rectangle.Height) ||
                IsNaN(rectangle.Width)
            );
        }

        /// <summary>
        /// Determines whether a double is not a number.
        /// </summary>
        /// <param name="value">
        /// The double value for which we want to determine whether it is not a number.
        /// </param>
        public static bool IsNaN(double value) {
            return double.IsNaN(value);
        }

        #endregion
    }
}