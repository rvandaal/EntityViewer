
using DiagramViewer.Models;
namespace DiagramViewer.ViewModels {
    public class ViewportViewModel : ViewModelBase {

        public UmlDiagram UmlDiagram { get; private set; }

        public ViewportViewModel(UmlModel model) {
            UmlDiagram = new UmlDiagram(model);
        }
    }
}
