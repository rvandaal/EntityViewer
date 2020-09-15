using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace DiagramViewer.Controls {
    /// <summary>
    /// Custom <see cref="ToolBarTray"/> which layouts its first child 
    /// only and gives it all the available space.
    /// </summary>
    /// <seealso cref="ToolBarTray">ToolBarTray Class</seealso>
    public class SingleChildToolBarTray : ToolBarTray {

        /// <summary>
        /// Static constructor.
        /// </summary>
        [SuppressMessage(
            "Microsoft.Performance",
            "CA1810:InitializeReferenceTypeStaticFieldsInline",
            Justification = "This is the correct way to override metadata in WPF."
        )]
        static SingleChildToolBarTray() {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SingleChildToolBarTray),
                new FrameworkPropertyMetadata(typeof(SingleChildToolBarTray))
            );
        }

        /// <summary>
        /// Arranges the first child to the complete given area.
        /// </summary>
        /// <param name="finalSize">
        /// The final area within the parent that this element 
        /// should use to arrange itself and its first child.
        /// </param>
        protected override Size ArrangeOverride(Size finalSize) {
            if (VisualChildrenCount > 0) {
                var visualChild = (UIElement)GetVisualChild(0);
                if (visualChild != null) {
                    visualChild.Arrange(new Rect(finalSize));
                }
            }
            return finalSize;
        }
    }
}
