
namespace DiagramViewer.Models {
    public class Link {

        public Link(Node startNode, Node endNode) {
            StartNode = startNode;
            EndNode = endNode;

            StartNode.AddLink(this);
            EndNode.AddLink(this);
        }

        public Node StartNode { get; set; }
        public Node EndNode { get; set; }
    }
}
