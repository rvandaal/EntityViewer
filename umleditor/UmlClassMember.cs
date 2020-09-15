
namespace UmlEditor {

    public enum AccessModifier {
        None,
        Private,
        Protected,
        ProtectedInternal,
        Internal,
        Public
    }

    public class UmlClassMember {
        public AccessModifier AccessModifier { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }

        public UmlClassMember(string name, string type = null, AccessModifier accessModifier = AccessModifier.None) {
            AccessModifier = accessModifier;
            Type = type;
            Name = name;
        }
    }
}
