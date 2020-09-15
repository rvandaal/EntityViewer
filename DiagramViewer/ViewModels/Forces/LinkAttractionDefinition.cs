using System;
using System.Windows;
using System.Windows.Media.Media3D;
using DiagramViewer.Utilities;

namespace DiagramViewer.ViewModels.Forces {
    public class LinkAttractionDefinition : ForceDefinition {

        private readonly ForceSetting attractionConstantSetting;

        public LinkAttractionDefinition() : base("Link attraction") {
            attractionConstantSetting = AddForceSetting("Attraction constant", 0, 1, 2, 0.1);
        }

        protected override void UpdateForcesOverride(Diagram diagram, double contentWidth, double contentHeight) {
            
            foreach (var diagramLink in diagram.Links) {

                double springLength = diagramLink.LabelWidth * 2;

                if (diagramLink.StartNode.ExertsForces && diagramLink.EndNode.ExertsForces) {
                    var force = CalcAttractionForce(
                        diagramLink, 
                        springLength, 
                        attractionConstantSetting.ParameterValue
                    ) * diagramLink.StartNode.ForceMultiplier * diagramLink.EndNode.ForceMultiplier;
                    if (diagramLink.StartNode.AcceptsForces) {
                        diagramLink.StartNode.AddForce(ForceType.LinkAttraction, force);
                    }
                    if (diagramLink.EndNode.AcceptsForces) {
                        diagramLink.EndNode.AddForce(ForceType.LinkAttraction, -force);
                    }
                }
            }
        }

        private static bool AreIntersecting(Point p1, Point p2, Point p3, Point p4) {
            var dx12 = p2.X - p1.X;
            var dy12 = p2.Y - p1.Y;
            var dx34 = p4.X - p3.X;
            var dy34 = p4.Y - p3.Y;
            var denominator = dy12 * dx34 - dx12 * dy34;
            if (DoubleUtility.AreClose(denominator, 0)) {
                return false;
            }
            var t1 = ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34) / denominator;
            var t2 = -((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12) / denominator;
            //var intersection = new Point(p1.X + dx12*t1, p1.Y + dy12*t1);
            return t1 >= 0 && t1 <= 1.0 && t2 >= 0.0 && t2 <= 1.0;
        }

        /// <summary>
        /// Calculates the attraction force between two connected nodes, using the specified spring length.
        /// </summary>
        private static Vector CalcAttractionForce(DiagramLink diagramLink, double springLength, double attractionConstant) {
            if (diagramLink.StartNode == diagramLink.EndNode) return new Vector(0, 0);
            var proximity = Math.Max(diagramLink.StartNode.GetDistanceToNode(diagramLink.EndNode), 0);
            //var proximity = CalcDistance(a, b);

            // Hooke's Law: F = -kx
            var force = attractionConstant * (proximity - springLength);
            //var force = ATTRACTION_CONSTANT * (proximity - springLength);
            //var angle = GetBearingAngle(a, b);
            //var angle = Math.Atan((b.Y - a.Y) / (b.X - a.X));

            //return new Vector(force * Math.Cos(angle), force * Math.Sin(angle));
            var vector = diagramLink.EndNode.Pos - diagramLink.StartNode.Pos;
            if (vector.Length > 0) {
                vector.Normalize();
            }
            return vector * force;
        }
    }
}
