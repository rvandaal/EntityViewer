
namespace DiagramViewer.ViewModels {
    public class TaskPanelViewModel : ViewModelBase {

        private UmlDiagramSimulator selectedUmlDiagramSimulator;
        public UmlDiagramSimulator SelectedUmlDiagramSimulator {
            get { return selectedUmlDiagramSimulator; }
            private set { SetPropertyClass(value, ref selectedUmlDiagramSimulator, () => SelectedUmlDiagramSimulator); }
        }

        private UmlDiagram selectedUmlDiagram;
        public UmlDiagram SelectedUmlDiagram {
            get { return selectedUmlDiagram; }
            private set { SetPropertyClass(value, ref selectedUmlDiagram, () => SelectedUmlDiagram); }
        }

        internal void SetSelectedViewportViewModel(ViewportViewModel viewportViewModel) {
            SelectedUmlDiagram = viewportViewModel.UmlDiagram;
            SelectedUmlDiagramSimulator = viewportViewModel.UmlDiagram.UmlDiagramSimulator;
        }
    }
}
