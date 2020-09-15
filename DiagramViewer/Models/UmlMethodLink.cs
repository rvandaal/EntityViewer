
namespace DiagramViewer.Models
{
    public class UmlMethodLink : Link
    {
        public UmlClass Class { get { return (UmlClass)StartNode; } }
        public UmlMethodNode MethodNode { get { return (UmlMethodNode)EndNode; } }

        public UmlMethodLink(UmlClass umlClass, UmlMethodNode umlMethodNode) : base(umlClass, umlMethodNode)
        {

        }
    }
}
