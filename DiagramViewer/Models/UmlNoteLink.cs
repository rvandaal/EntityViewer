
namespace DiagramViewer.Models {
    public class UmlNoteLink : Link {
        public UmlClass Class { get { return (UmlClass) StartNode; } }
        public UmlNote Note { get { return (UmlNote) EndNode; } }

        public UmlNoteLink(UmlClass umlClass, UmlNote umlNote) : base(umlClass, umlNote) {
            
        }
    }
}
