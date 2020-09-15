
namespace DiagramViewer.Models {
    public class UmlProperty : UmlClassMember {

        public AccessModifier GetterAccessModifier { get; private set; }
        public AccessModifier SetterAccessModifier { get; private set; }

        public UmlProperty(
            string name, 
            string type = null,
            AccessModifier getterAccessModifier = AccessModifier.None, 
            AccessModifier setterAccessModifier = AccessModifier.None
        ) : base(name, type) {
            GetterAccessModifier = getterAccessModifier;
            SetterAccessModifier = setterAccessModifier;
        }
    }
}
