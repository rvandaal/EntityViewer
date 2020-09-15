using UmlViewer.Controllers;

namespace UmlViewer.ViewModels {
    
    public class MainWindowViewModel : ViewModelBase {
        
        #region Private fields
        private MainWindowController controller;
        public ViewAreaViewModel ViewAreaViewModel { get; private set; }
        #endregion

        #region Constructors

        public MainWindowViewModel(MainWindowController mainWindowController) {
            controller = mainWindowController;
            ViewAreaViewModel = new ViewAreaViewModel();
        }

        #endregion
    }
}
