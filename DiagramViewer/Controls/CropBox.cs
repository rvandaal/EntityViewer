using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace DiagramViewer.Controls {

    /// <summary>
    /// A decorator that can crop its child at one of its sides.
    /// </summary>
    /// <remarks>
    /// The main usage of this control is in animations where the contents should gradually appear
    /// and disappear via a sliding movement.
    /// </remarks>
    /// <seealso cref="Decorator">Decorator Class</seealso>
    public class CropBox : Decorator {

        #region CropFactorChanged RoutedEvent
        /// <summary>
        /// Custom RoutedEvent registration for the <see cref="CropFactorChanged"/> event.
        /// </summary>
        public static readonly RoutedEvent CropFactorChangedEvent =
            EventManager.RegisterRoutedEvent(
                "CropFactorChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(CropBox)
            );

        /// <summary>
        /// Adds or removes a handler for the <see cref="CropFactorChangedEvent"/>.
        /// </summary>
        public event RoutedEventHandler CropFactorChanged {
            add => AddHandler(CropFactorChangedEvent, value);
            remove => RemoveHandler(CropFactorChangedEvent, value);
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Static constructor.
        /// </summary>
        [SuppressMessage(
            "Microsoft.Performance",
            "CA1810:InitializeReferenceTypeStaticFieldsInline",
            Justification = "This is the correct way to override metadata in WPF."
        )]
        static CropBox() {
            ClipToBoundsProperty.OverrideMetadata(typeof(CropBox), new PropertyMetadata(true));
            FocusableProperty.OverrideMetadata(
                typeof(CropBox),
                new FrameworkPropertyMetadata(false)
            );
        }

        #endregion

        #region Public members

        #region CropFactor

        /// <summary>
        /// Gets or sets the crop factor.
        /// </summary>
        /// <remarks>
        /// The crop factor should be in the range [0.0, 1.0]. A crop factor of 0.0 indicates
        /// that no cropping will be done.
        /// </remarks>
        public double CropFactor {
            get => (double)GetValue(CropFactorProperty);
            set => SetValue(CropFactorProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="CropFactor"/> dependency property.
        /// </summary>
        /// <value>
        /// The default value of this property is 0.0.
        /// </value>
        public static readonly DependencyProperty CropFactorProperty =
            DependencyProperty.Register(
                "CropFactor",
                typeof(double),
                typeof(CropBox),
                new FrameworkPropertyMetadata(
                    0d,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnCropFactorChanged,
                    CoerceCropFactor
                )
            );

        #endregion

        #region CropSide

        /// <summary>
        /// Gets or sets the side at which cropping takes places.
        /// </summary>
        /// <remarks>
        /// This property is often the opposite of the slide direction. For example, when an
        /// expander expands to the bottom, the <b>CropSide</b> will be <see cref="Side.Top"/>.
        /// </remarks>
        public Side CropSide {
            get => (Side)GetValue(CropSideProperty);
            set => SetValue(CropSideProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="CropSide"/> dependency property.
        /// </summary>
        /// <value>
        /// The default value of this property is <see cref="Side.Top"/>.
        /// </value>
        public static readonly DependencyProperty CropSideProperty =
            DependencyProperty.Register(
                "CropSide",
                typeof(Side),
                typeof(CropBox),
                new FrameworkPropertyMetadata(
                    Side.Top,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange
                )
            );

        #endregion

        #endregion

        #region Protected members
        /// <summary>
        /// Measures the crop box based on the values of <see cref="CropFactor"/> and
        /// <see cref="CropSide"/>.
        /// </summary>
        /// <param name="constraint">The maximum size that the method can return.</param>
        protected override Size MeasureOverride(Size constraint) {
            //
            // Call the base MeasureOverride to calculate the available size if no cropping
            // would have to be done. In this override, the Child is measured independent of
            // the CropFactor.
            //
            var size = base.MeasureOverride(constraint);
            //
            // If there is nothing to crop, the available size, as calculated by the base
            // Measure method, can be used.
            //
            if (CropFactor > 0) {
                size = CalculateCropSize(CropFactor, CropSide, size);
            }
            //
            // Return the calculated size, which is used in the ArrangeOverride method.
            //
            return size;
        }

        /// <summary>
        /// Arranges the child of the crop box taking the <see cref="CropFactor"/> 
        /// and the <see cref="CropSide"/> into account.
        /// </summary>
        /// <param name="finalSize">
        /// The size of the final area within the parent that this element should use to arrange 
        /// its child.
        /// </param>
        protected override Size ArrangeOverride(Size finalSize) {
            //
            // If there is nothing to crop, use the standard Arrange behavior.
            //
            if (CropFactor == 0) {
                return base.ArrangeOverride(finalSize);
            }

            if (Child != null) {
                var arrangement =
                    CalculateCroppedArrangement(
                        CropFactor,
                        CropSide,
                        Child.DesiredSize,
                        finalSize
                    );
                //
                // Propagate the calculated arrangement to the Child.
                //
                Child.Arrange(arrangement);
            }
            return finalSize;
        }

        #endregion

        #region Private members

        /// <summary>
        /// Handles changes to the <see cref="CropFactor"/> property.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> on which the <see cref="CropFactor"/> was set.
        /// </param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/>.</param>
        private static void OnCropFactorChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e
        ) {
            var cropBox = (CropBox)dependencyObject;
            cropBox.OnCropFactorChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        /// Handles changes to the <see cref="CropFactor"/> property.
        /// </summary>
        /// <param name="oldCropFactor">The old value of the <see cref="CropFactor"/>.</param>
        /// <param name="newCropFactor">The new value of the <see cref="CropFactor"/>.</param>
        /// <remarks>
        /// This event handler raises the <see cref="CropFactorChangedEvent"/> if the new
        /// <see cref="CropFactor"/> is 0 or 1 or if the new CropFactor is between 0 and 1 and the 
        /// old CropFactor is 0 or 1. This <see cref="RoutedEvent"/> can be handled by 
        /// the application to check if the contents of the <see cref="CropBox"/> are fully 
        /// cropped or not cropped at all.
        /// </remarks>
        private void OnCropFactorChanged(double oldCropFactor, double newCropFactor) {
            if (newCropFactor == 0) {
                //
                // There is nothing to crop. However, we do attempt to bring this element into
                // view. This is necessary because expanding the element may shift it out of
                // view. See, for example, MIP00103430 "PT: Datagrid: Selecting the last visible
                // row in DataGrid does not show its rowdetail completely".
                //
                BringIntoView();
            }
            //
            // Only raise a event if there is a significant change. In case the CropFactor is 
            // animated, the event is raised once the animation starts and as soon as the animation
            // stops.
            //
            if (
                (newCropFactor == 0) ||
                (newCropFactor == 1) ||
                (oldCropFactor == 0) ||
                (oldCropFactor == 1)
            ) {
                RaiseEvent(new RoutedEventArgs(CropFactorChangedEvent, this));
            }
        }

        /// <summary>
        /// Provides a CoerceValueCallback for the <see cref="CropFactor"/> 
        /// to ensure that it is within its bounds.
        /// </summary>
        private static object CoerceCropFactor(DependencyObject d, object value) {
            var current = (double)value;
            if (current < 0) {
                current = 0;
            } else if (current > 1) {
                current = 1;
            }
            return current;
        }

        /// <summary>
        /// Calculates the cropped size based on the crop factor, crop side and available size.
        /// </summary>
        /// <param name="cropFactor">The crop factor.</param>
        /// <param name="cropSide">The crop side.</param>
        /// <param name="availableSize">The available size.</param>
        private static Size CalculateCropSize(
            double cropFactor,
            Side cropSide,
            Size availableSize
        ) {
            var desiredSize = availableSize;

            // Determine the cropped size for the content
            // A CropFactor of 0 means nothing is cropped; return the available size in that case.
            if (cropFactor > 0) {
                if ((cropSide == Side.Left) || (cropSide == Side.Right)) {
                    desiredSize.Width = availableSize.Width * (1 - cropFactor);
                } else {
                    desiredSize.Height = availableSize.Height * (1 - cropFactor);
                }
            }
            return desiredSize;
        }

        /// <summary>
        /// Calculates the arrangement for the cropped content.
        /// </summary>
        /// <param name="cropFactor">The crop factor.</param>
        /// <param name="cropSide">The crop side.</param>
        /// <param name="desiredContentSize">The desired size for the cropped content.</param>
        /// <param name="finalSize">
        /// The final area within the parent that this element should use to arrange itself
        /// and its children.
        /// </param>
        private static Rect CalculateCroppedArrangement(
            double cropFactor,
            Side cropSide,
            Size desiredContentSize,
            Size finalSize
        ) {
            // First determine what the size would be if not cropped
            Rect rect;
            if (cropFactor == 1) {
                rect = new Rect(0, 0, 0, 0);
            } else {
                rect = new Rect {
                    Width = Math.Max(desiredContentSize.Width, finalSize.Width),
                    Height = Math.Max(desiredContentSize.Height, finalSize.Height)
                };

                // The position of the Child depends on the CropSide. The offset is relatively to 
                // the top/left corner so if the crop side is right or bottom, the Child should be 
                // aligned with top/left: in this case, an offset is not necessary.
                switch (cropSide) {
                    case Side.Top:
                        //This keeps the child bottom aligned
                        rect.Offset(0, finalSize.Height - desiredContentSize.Height);
                        break;
                    case Side.Left:
                        //This keeps the Child right aligned
                        rect.Offset(finalSize.Width - desiredContentSize.Width, 0);
                        break;
                    default:
                        break;
                }
            }
            return rect;
        }

        #endregion
    }
}
