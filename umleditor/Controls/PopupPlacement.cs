using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows;

namespace UmlEditor.Controls {
    public enum HorizontalPlacement { Left, Right, Center };

    public enum VerticalPlacement { Top, Bottom, Center };

    /// <summary>
    /// In WPF, PopUps pop up in different places on different machines (due to different "handedness" on touch-enabled screens.  This fixes it.
    /// See Also: http://social.msdn.microsoft.com/Forums/vstudio/en-US/19ef3d33-01e5-45c5-a845-d64f9231001c/popup-positioningalignments?forum=wpf
    /// </summary>
    public static class PopupPlacement {
        /// <summary>
        /// Usage: In XAML, add the following to your tooltip: 
        ///     Placement="Custom" CustomPopupPlacementCallback="CustomPopupPlacementCallback" 
        /// and call this method from the CustomPopupPlacementCallback.
        /// </summary>
        public static CustomPopupPlacement[] PlacePopup(Size popupSize, Size targetSize, Point offset, VerticalPlacement verticalPlacement, HorizontalPlacement horizontalPlacement) {
            Point p = new Point {
                X = GetHorizontalOffset(popupSize, targetSize, horizontalPlacement),
                Y = GetVerticalOffset(popupSize, targetSize, verticalPlacement)
            };

            return new[] {
                new CustomPopupPlacement(p, PopupPrimaryAxis.Horizontal)
            };
        }

        private static double GetVerticalOffset(Size popupSize, Size targetSize, VerticalPlacement verticalPlacement) {
            switch (verticalPlacement) {
                case VerticalPlacement.Top:
                    return -popupSize.Height;
                case VerticalPlacement.Bottom:
                    return targetSize.Height;
                case VerticalPlacement.Center:
                    return -(popupSize.Height / 2) + targetSize.Height / 2;
            }

            throw new ArgumentOutOfRangeException("verticalPlacement");
        }

        private static double GetHorizontalOffset(Size popupSize, Size targetSize, HorizontalPlacement horizontalPlacement) {
            switch (horizontalPlacement) {
                case HorizontalPlacement.Left:
                    return 0;
                case HorizontalPlacement.Right:
                    return targetSize.Width - popupSize.Width;
                case HorizontalPlacement.Center:
                    return -(popupSize.Width / 2) + targetSize.Width / 2;
            }
            throw new ArgumentOutOfRangeException("horizontalPlacement");
        }
    }
}
