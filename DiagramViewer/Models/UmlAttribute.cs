
namespace DiagramViewer.Models {
    public class UmlAttribute : UmlClassMember {
        public AccessModifier AccessModifier { get; set; }
        public UmlAttribute(string name, string type = null, AccessModifier accessModifier = AccessModifier.None) : base(name, type) {
            AccessModifier = accessModifier;
        }
    }
}
