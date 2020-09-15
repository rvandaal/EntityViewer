using System.Reflection;

namespace DiagramViewer.Models {
    public class UmlDependenceRelation : UmlAssociationRelation {
        public UmlDependenceRelation(UmlClass startClass, UmlClass endClass, string name = null, string startMultiplicity = "1", string endMultiplicity = "1", MemberInfo memberInfo = null) : 
            base(startClass, endClass, name, startMultiplicity, endMultiplicity) { }
    }
}
