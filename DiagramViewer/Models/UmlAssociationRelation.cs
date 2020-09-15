

using Mono.Cecil;

namespace DiagramViewer.Models {
    public class UmlAssociationRelation : UmlRelation {
        public MemberReference MemberReference { get; private set; }

        public UmlAssociationRelation (
            UmlClass startClass, 
            UmlClass endClass, 
            string name = null, 
            string startMultiplicity = "1", 
            string endMultiplicity = "1",
            MemberReference memberReference = null
        ) : base(startClass, endClass, name, startMultiplicity, endMultiplicity) {
            MemberReference = memberReference;
        }
    }
}
