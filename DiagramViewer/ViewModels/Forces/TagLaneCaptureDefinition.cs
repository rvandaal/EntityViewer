using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DiagramViewer.ViewModels.Forces {
    public class TagLaneCaptureDefinition : ForceDefinition {

        public TagLaneCaptureDefinition() : base("Tag lanes") {
            
        }

        protected override void UpdateForcesOverride(Diagram diagram, double contentWidth, double contentHeight) {
            return;
            List<string> visibleNodeTags = new List<string>();
            foreach(var node in diagram.Nodes) {
                if(node.Tags.Any()) {
                    foreach(var tag in node.Tags) {
                        if(!visibleNodeTags.Contains(tag)) {
                            visibleNodeTags.Add(tag);
                        }
                    }
                }
            }
            var laneCount = visibleNodeTags.Count;
            var laneWidth = contentWidth/laneCount;
            int i = 0;

            foreach(var tag in visibleNodeTags) {
                var laneX = (i + 0.5)*laneWidth;
                i++;
                string tag1 = tag;
                foreach(var node in diagram.Nodes.Where(n => n.Tags.Contains(tag1))) {
                    var distanceToLaneX = node.TopLeft.X - laneX;
                    node.AddForce(ForceType.TagLaneCapture, new Vector(-1,0) * distanceToLaneX * 0.5);
                }
            }
        }
    }
}
