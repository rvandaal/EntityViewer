using DiagramViewer.Models;
using System.ComponentModel;

namespace DiagramViewer.ViewModels {
    public class MainWindowViewModel : ViewModelBase {
        private UmlModel model;

        public ViewAreaViewModel ViewAreaViewModel { get; private set; }
        public TaskPanelViewModel TaskPanelViewModel { get; private set; }

        public MainWindowViewModel() {
            model = new UmlModel();
            ViewAreaViewModel = new ViewAreaViewModel(model);
            TaskPanelViewModel = new TaskPanelViewModel();
            ViewAreaViewModel.PropertyChanged += OnViewAreaPropertyChanged;
            OnViewAreaPropertyChanged(null, null);
        }

        private void OnViewAreaPropertyChanged(object sender, PropertyChangedEventArgs e) {
            TaskPanelViewModel.SetSelectedViewportViewModel(ViewAreaViewModel.SelectedViewportViewModel);
        }
    }
}
