
namespace DiagramViewer.Models {
    public class UmlRelation : Link {
        public UmlClass StartClass { get { return (UmlClass) StartNode; } }
        public UmlClass EndClass { get { return (UmlClass) EndNode; } }
        public string StartMultiplicity { get; set; }
        public string EndMultiplicity { get; set; }
        public string Label { get; set; }

        public UmlRelation(
            UmlClass startClass, 
            UmlClass endClass, 
            string name = null, 
            string startMultiplicity = "1", 
            string endMultiplicity = "1"
        ) : base(startClass, endClass) {
            Label = name;
            StartClass.AddRelation(this);
            EndClass.AddRelation(this);
            StartMultiplicity = startMultiplicity;
            EndMultiplicity = endMultiplicity;
        }
    }
}
