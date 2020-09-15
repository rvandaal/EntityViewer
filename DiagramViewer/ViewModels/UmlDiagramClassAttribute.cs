using DiagramViewer.Models;

namespace DiagramViewer.ViewModels {
    public class UmlDiagramClassAttribute : UmlDiagramClassMember {

        public UmlAttribute UmlAttribute { get; set; }
        public UmlDiagramClassAttribute(UmlAttribute umlAttribute) : base(umlAttribute) {
            UmlAttribute = umlAttribute;
        }

        public AccessModifier AccessModifier {
            get { return UmlAttribute.AccessModifier; }
        }
    }
}
