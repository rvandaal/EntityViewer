
using System.Reflection;
using Mono.Cecil;

namespace DiagramViewer.Models {
    public class UmlOperation : UmlClassMember {
        public AccessModifier AccessModifier { get; set; }
        public UmlOperation(string name, string type = null, AccessModifier accessModifier = AccessModifier.None) : base(name, type) {
            AccessModifier = accessModifier;
        }
        public MethodDefinition Method { get; set; }
    }
}