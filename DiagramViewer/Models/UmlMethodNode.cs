
namespace DiagramViewer.Models
{
    public class UmlMethodNode : Node
    {
        public string MethodName { get; set; }

        public UmlMethodNode(string methodName)
        {
            MethodName = methodName;
        }
    }
}
