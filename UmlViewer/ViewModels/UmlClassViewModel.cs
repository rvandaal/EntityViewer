using UmlViewer.Controllers;

namespace UmlViewer.ViewModels {
    public class UmlClassViewModel {
        public UmlClassController Controller { get; private set; }
        public UmlClassViewModel(UmlClassController controller) {
            Controller = controller;
        }
    }
}
