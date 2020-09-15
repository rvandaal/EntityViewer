using System.Windows.Controls;

namespace DiagramViewer.Controls {
    public class ViewArea : ListBox {
        protected override System.Windows.DependencyObject GetContainerForItemOverride() {
            return new Viewport();
        }
        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is Viewport;
        }
    }
}
