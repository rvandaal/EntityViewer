using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using UmlViewer.Controllers;

namespace UmlViewer.ViewModels {
    public class UmlDiagramViewModel : ViewModelBase {
        private readonly UmlDiagramController controller;
        public UmlDiagramViewModel(UmlDiagramController controller) {
            this.controller = controller;
            controller.UmlClassControllers.CollectionChanged += OnUmlClassesChanged;
            InitUmlClassViewModels();
        }

        private readonly ObservableCollection<UmlClassViewModel> umlClassViewModels = 
            new ObservableCollection<UmlClassViewModel>();

        public ObservableCollection<UmlClassViewModel> UmlClassViewModels {
            get { return umlClassViewModels; }
        }

        private void InitUmlClassViewModels() {
            foreach (var umlClassController in controller.UmlClassControllers) {
                var viewModel = new UmlClassViewModel(umlClassController);
                umlClassViewModels.Add(viewModel);
            }
        }

        private void OnUmlClassesChanged(object sender, NotifyCollectionChangedEventArgs e) {
            foreach (var removedItem in e.OldItems) {
                var viewModel = umlClassViewModels.FirstOrDefault(v => v.Controller == removedItem);
                umlClassViewModels.Remove(viewModel);
            }

            foreach (var addedItem in e.NewItems) {
                var viewModel = new UmlClassViewModel((UmlClassController)addedItem);
                umlClassViewModels.Add(viewModel);
            }
        }
    }
}
