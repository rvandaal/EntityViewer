using System;
using System.Linq;
using System.Windows;

namespace DiagramViewer.ViewModels.Forces {
    public class NodeAttractionDefinition : ForceDefinition {

        private readonly ForceSetting attractionConstantSetting;

        public NodeAttractionDefinition() : base("Node attraction") {
            attractionConstantSetting = AddForceSetting("Attraction constant", 0, 0.1, 3, 0.01);
        }

        protected override void UpdateForcesOverride(Diagram diagram, double contentWidth, double contentHeight) {
            foreach (var diagramNode1 in diagram.ForceAcceptingNodes) {
                DiagramNode tmpDiagramNode = diagramNode1;
                foreach (var diagramNode2 in diagram.ForceExertingNodes.Where(n => n != tmpDiagramNode)) {
                    diagramNode1.AddForce(
                        ForceType.NodeAttraction, 
                        CalcAttractionForce(diagramNode1, diagramNode2, 100.0, attractionConstantSetting.ParameterValue) * diagramNode1.ForceMultiplier * diagramNode2.ForceMultiplier
                    );
                }
            }
        }

        /// <summary>
        /// Calculates the attraction force between two connected nodes, using the specified spring length.
        /// </summary>
        private static Vector CalcAttractionForce(
            DiagramNode diagramNode1, 
            DiagramNode diagramNode2, 
            double springLength, 
            double attractionConstant
        ) {
            if (diagramNode1 == diagramNode2) return new Vector(0, 0);
            var proximity = Math.Max((diagramNode2.Pos - diagramNode1.Pos).Length, 0);

            // Hooke's Law: F = -kx
            var force = attractionConstant * (proximity - springLength);
            var vector = diagramNode2.Pos - diagramNode1.Pos;
            if (vector.Length > 0) {
                vector.Normalize();
            }
            return vector * force;
        }
    }
}
