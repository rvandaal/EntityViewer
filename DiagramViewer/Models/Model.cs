using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DiagramViewer.Models {
    public class Model {

        public Model() {
            nodes = new List<Node>();
            Nodes = nodes.AsReadOnly();
            links = new List<Link>();
            Links = links.AsReadOnly();
        }

        public virtual void ClearModel() {
            nodes.Clear();
            links.Clear();
        }

        #region Nodes

        public ReadOnlyCollection<Node> Nodes { get; private set; }
        private readonly List<Node> nodes;

        public void AddNode(Node node) {
            if (!nodes.Contains(node)) {
                nodes.Add(node);
            }
        }

        public void RemoveNode(Node node) {
            if (nodes.Contains(node)) {
                nodes.Remove(node);
            }
        }

        #endregion

        #region Links

        public ReadOnlyCollection<Link> Links { get; private set; }
        private readonly List<Link> links;

        public void AddLink(Link link) {
            if (!links.Contains(link)) {
                links.Add(link);
            }
        }

        public void RemoveLink(Link link) {
            if (links.Contains(link)) {
                links.Remove(link);
            }
        }

        #endregion
    }
}
