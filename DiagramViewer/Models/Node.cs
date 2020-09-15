using System.Collections.Generic;

namespace DiagramViewer.Models {
    public class Node {

        public Node() {
           links = new List<Link>();
        }

        private readonly List<Link> links;

        public IList<Link> Links {
            get { return links.AsReadOnly(); }
        }

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
    }
}
