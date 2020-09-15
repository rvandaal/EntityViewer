using System.Collections.ObjectModel;
using DiagramViewer.Models;

namespace DiagramViewer.ViewModels {
    public class ViewAreaViewModel : ViewModelBase {

        private UmlModel model;

        private readonly ObservableCollection<ViewportViewModel> viewportViewModels = new ObservableCollection<ViewportViewModel>();
        public ObservableCollection<ViewportViewModel> ViewportViewModels {
            get { return viewportViewModels; }
        }

        private ViewportViewModel selectedViewportViewModel;
        public ViewportViewModel SelectedViewportViewModel {
            get { return selectedViewportViewModel; }
            set { SetPropertyClass(value, ref selectedViewportViewModel, () => SelectedViewportViewModel); }
        }

        public ViewAreaViewModel(UmlModel model) {
            this.model = model;
            //ViewportViewModels.Add(new ViewportViewModel(model));
            //ViewportViewModels.Add(new ViewportViewModel(model));
            //ViewportViewModels.Add(new ViewportViewModel(model));
            ViewportViewModels.Add(new ViewportViewModel(model));
            selectedViewportViewModel = ViewportViewModels[0];
        }
    }
}
