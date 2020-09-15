using ZoomAndPan;
using ViewModelBase = GraphFramework.ViewModelBase;

namespace UmlEditor {
    public class MainWindowViewModel : ViewModelBase {
        private MainWindowController controller;

        public ZoomAndPanViewModel ZoomAndPanViewModel { get; private set; }

        public MainWindowViewModel(MainWindowController controller) {
            this.controller = controller;
            ZoomAndPanViewModel = new ZoomAndPanViewModel();
        }

        public UmlDiagram Diagram {
            get { return controller.Diagram; }
        }
    }
}
