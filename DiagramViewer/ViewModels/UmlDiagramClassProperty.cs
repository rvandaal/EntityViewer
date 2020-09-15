using DiagramViewer.Models;

namespace DiagramViewer.ViewModels {
    public class UmlDiagramClassProperty : UmlDiagramClassMember {

        public UmlProperty UmlProperty { get; set; }
        public UmlDiagramClassProperty(UmlProperty umlProperty)
            : base(umlProperty) {
            UmlProperty = umlProperty;
        }

        public AccessModifier GetterAccessModifier {
            get { return UmlProperty.GetterAccessModifier; }
        }

        public AccessModifier SetterAccessModifier {
            get { return UmlProperty.SetterAccessModifier; }
        }
    }
}
