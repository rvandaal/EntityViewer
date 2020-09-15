using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UmlViewer.Models {
    public class Session {
        // contains umlmodel and all ui settings
        public UmlModel UmlModel { get; private set; }

        public Session() {
            UmlModel = new UmlModel();
        }
    }
}
