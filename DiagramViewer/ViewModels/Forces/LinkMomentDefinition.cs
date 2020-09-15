using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Media3D;

namespace DiagramViewer.ViewModels.Forces {
    public class LinkMomentDefinition : ForceDefinition {

        private readonly ForceSetting linkMomentConstantSetting;

        public LinkMomentDefinition() : base("Link moment") {
            linkMomentConstantSetting = AddForceSetting("Link moment constant", 0, 0.0001, 5, 0.00005);
        }

        protected override void UpdateForcesOverride(Diagram diagram, double contentWidth, double contentHeight) {

            foreach (var umlDiagramRelation in diagram.Links) {
                UpdateDiscreteAngleForces(umlDiagramRelation, linkMomentConstantSetting.ParameterValue);
            }
        }

        public virtual void UpdateDiscreteAngleForces(DiagramLink diagramLink, double linkMomentConstant) {

            if (diagramLink.PreferredAngles == null || diagramLink.PreferredAngles.Length == 0) return;

            var startNode = diagramLink.StartNode;
            var endNode = diagramLink.EndNode;

            var vectorToConnectedNode = endNode.Pos - startNode.Pos;
            var vectorToConnectedNode3D = new Vector3D(vectorToConnectedNode.X, vectorToConnectedNode.Y, 0);
            vectorToConnectedNode.Normalize();
            var rotateClockwiseVector3D = Vector3D.CrossProduct(vectorToConnectedNode3D, new Vector3D(0, 0, 1));
            var rotateCounterClockwiseVector3D = Vector3D.CrossProduct(vectorToConnectedNode3D, new Vector3D(0, 0, -1));
            var rotateClockwiseVector = new Vector(rotateClockwiseVector3D.X, rotateClockwiseVector3D.Y);
            var rotateCounterClockwiseVector = new Vector(rotateCounterClockwiseVector3D.X, rotateCounterClockwiseVector3D.Y);
            double angle0 = diagramLink.Angle0;

            // Try to refactor enum into array of doubles
            double[] preferredAngles = diagramLink.PreferredAngles;
            double angle;
            double currentPreferredAngle = GetPreferredAngle(angle0, preferredAngles, out angle);
            bool rotateClockwise = angle0 - currentPreferredAngle > 0;

            // Let's say the preferred angles are 45, 135, 225 and 315
            // Then the link forces should be continuous around these angles.
            // This is currently not the case when we choose preferred angle and disrespect the rest of the angles.
            // Then we get a labiel evenwicht: one small movement to the left or right of angle 0, and the
            // link will be drawn towards 45 or -45 degrees.
            // To prevent discontinuities, we cannot choose a preferred angle: all the angles should always exert a force.
            // This only works if we define the angles between the preferred angles (in this example, 0, 90, 180, 270),
            // if those angles define the moment forces on the link and if the forces increase if the angle gets bigger.
            // 

            var vector = rotateClockwise ? rotateClockwiseVector : rotateCounterClockwiseVector;

            var torsionForce = angle * linkMomentConstant;
            var force = torsionForce * diagramLink.HalfLength; // Force is exerted from the middle of the link

            startNode.AddForce(ForceType.DiscreteAngles, vector * force);
            endNode.AddForce(ForceType.DiscreteAngles, -vector * force);
        }

        private static double GetPreferredAngle(double currentAngle, IEnumerable<double> preferredAngles, out double deltaAngle) {
            double minDeltaAngle = double.PositiveInfinity;
            double preferredAngle = double.NaN;
            foreach (double currentPreferredAngle in preferredAngles) {
                double currentDeltaAngle = GetAngleInBetween(currentAngle, currentPreferredAngle);
                if (currentDeltaAngle < minDeltaAngle) {
                    minDeltaAngle = currentDeltaAngle;
                    preferredAngle = currentPreferredAngle;
                }
            }
            deltaAngle = minDeltaAngle;
            return preferredAngle;
        }

        private static double GetAngleInBetween(double angle1, double angle2) {
            double delta = Math.Abs(angle1 - angle2);
            if (delta > 180) delta = 360 - delta;
            return delta;
        }
    }
}
