using System;
using System.Linq;
using System.Windows;

namespace DiagramViewer.ViewModels.Forces {
    public class Node2NodeRepulsionDefinition : ForceDefinition {

        private readonly ForceSetting repulsionForceSetting;

        public Node2NodeRepulsionDefinition() : base("Node repulsion") {
            repulsionForceSetting = AddForceSetting("Repulsion constant", 0, 10e5, 0, 5e5);
        }

        protected override void UpdateForcesOverride(Diagram diagram, double contentWidth, double contentHeight) {
            foreach (var node1 in diagram.ForceAcceptingNodes) {
                DiagramNode tmpNode = node1;
                foreach (var node2 in diagram.ForceExertingNodes.Where(n => n != tmpNode)) {
                    node1.AddForce(ForceType.Repulsion, CalcRepulsionForce(node1, node2) * node1.ForceMultiplier * node2.ForceMultiplier);
                }
            }
        }
        
        private Vector CalcRepulsionForce(DiagramNode a, DiagramNode b) {
            if (a == b) return new Vector(0, 0);
            double distance = a.GetDistanceToNode(b); // ik vraag me af of GetDistanceToNode wel slim is om te gebruiken, forces zijn hier door discreet, want hij bepaalt
            // eerst bij elke hoek hij het dichtste bij is.
            

            //double distance = (a.Pos - b.Pos).Length;
            var proximity = Math.Max(distance, 1);

            // Onderstaande code stond eerst aan maar zorgt voor discrete overgangen, forces die ineens omflippen
            //if (proximity > GetDefaultRepulsionHorizon(a, b)) {
            //    return new Vector(0, 0);
            //}

            // Coulomb's Law: F = k(Qq/r^2)
            var force = -(GetRepulsionConstant(a, b) / Math.Pow(proximity, 2));
            //var angle = GetBearingAngle(a, b);

            //return new Vector(force * Math.Cos(angle), force * Math.Sin(angle));
            var vector = b.Pos - a.Pos;
            if (vector.Length > 0) {
                vector.Normalize();
            }

            force = Math.Min(1e4, Math.Max(-1e4, force));
            return vector * force;
        }

        private static double GetDefaultRepulsionHorizon(DiagramNode diagramNode1, DiagramNode diagramNode2) {
            if (diagramNode1 is UmlDiagramClass && diagramNode2 is UmlDiagramNote || diagramNode2 is UmlDiagramClass && diagramNode1 is UmlDiagramNote) {
                return 100.0;
            }
            return 200.0; //500
        }

        private double GetRepulsionConstant(DiagramNode diagramNode1, DiagramNode diagramNode2) {
            if (diagramNode1 is UmlDiagramClass && diagramNode2 is UmlDiagramNote || diagramNode2 is UmlDiagramClass && diagramNode1 is UmlDiagramNote) {
                return 1e5;
            }
            return repulsionForceSetting.ParameterValue;
        }
    }
}
