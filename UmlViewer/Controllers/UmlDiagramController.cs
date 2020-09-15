using System.Collections.Generic;
using System.Collections.ObjectModel;
using UmlViewer.Models;

namespace UmlViewer.Controllers {
    public class UmlDiagramController {
        private UmlModel model;

        public ObservableCollection<UmlClassController> UmlClassControllers { get; private set; }

        public UmlDiagramController(UmlModel model) {
            this.model = model;
            List<UmlClassController> controllers = new List<UmlClassController>();
            foreach(var umlClass in model.UmlClasses) {
                controllers.Add(new UmlClassController(umlClass));
            }
            UmlClassControllers = new ObservableCollection<UmlClassController>(controllers);
        }
    }
}
