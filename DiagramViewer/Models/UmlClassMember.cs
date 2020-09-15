
namespace DiagramViewer.Models {

    public enum AccessModifier {
        None,
        Private,
        Protected,
        ProtectedInternal,
        Internal,
        Public
    }

    public class UmlClassMember {
        
        public string Type { get; set; }
        public string Name { get; set; }

        public UmlClassMember(string name, string type = null) {
            Type = type;
            Name = name;
        }
    }
}
