using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmlViewer.Controllers;

namespace UmlViewer.ViewModels {
    class UmlRelationViewModel : ViewModelBase {
        private UmlRelationController controller;

        public UmlRelationViewModel(UmlRelationController controller) {
            this.controller = controller;
        }
    }
}
