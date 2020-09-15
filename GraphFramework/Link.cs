using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace GraphFramework {

    public class Link {
        //
        // 24-8-2015: goed idee: links niet als rechte lijnen zien maar als bezierkromme's die toevallig ook recht kunnen zijn.

        public static int StaticIndex { get; set; }

        public int Index { get; set; }

        public Link() : this("0d") {}

        public Link(string preferredAngleString) {
            SetPreferredAngles(preferredAngleString);
            Index = StaticIndex++;
            IsVisible = true;
        }

        public void SetPreferredAngles(string preferredAngleString) {
            string[] preferredAngleStringArray = preferredAngleString.Split(new[] { ',', ' ' },
                                                                            StringSplitOptions.RemoveEmptyEntries);

            PreferredAngles = new double[preferredAngleStringArray.Length];
            List<double> angleList = new List<double>();
            foreach (var angleString in preferredAngleStringArray) {
                angleList.Add(double.Parse(angleString));
            }
            PreferredAngles = angleList.ToArray();
        }

        public bool IsSimulated { get { return !IsBeingDragged; } }
        public bool IsBeingDragged { get; set; }
        public bool IsVisible { get; set; }

        public Node StartNode { get; set; }
        public Node EndNode { get; set; }

        public double[] PreferredAngles { get; set; }

        //public string Label { get { return EndNode != null ? string.Format("{0:f0}", StartNode.GetDistanceToNode(EndNode)) : ""; } }
        public string Label { get; set; }

        public Point ManualStartPoint { get; set; }
        public Point ManualEndPoint { get; set; }

        public Point StartPoint {
            get {
                if (StartNode != null) {
                    return StartNode.Pos2D;
                }
                return ManualStartPoint;
            }
        }

        public Point EndPoint {
            get {
                if (EndNode != null) {
                    return EndNode.Pos2D;
                }
                return ManualEndPoint;
            }
        }

        public Point HalfwayPosition {
            get {
                return StartPoint + (EndPoint - StartPoint) / 2; }
        }

        public virtual double Angle0 {
            get {
                //
                // Note: this angle (position of end point) goes from 0 to 180, flips to -180 and goes back to 0.
                //
                return Vector.AngleBetween((EndPoint - StartPoint), Utils.Angle0Vector);
            }
        }

        public virtual double Angle90 {
            //
            // Make the 90 vector negative here, so Angle90 is the angle between the upvector and the angle.
            // TODO: we could also negate the Angle90Vector in the Utils. Not sure about the dependencies.
            //
            get { return Vector.AngleBetween((EndPoint - StartPoint), -Utils.Angle90Vector); }
        }

        public double HalfLength {
            get { return (EndNode.Pos2D - StartNode.Pos2D).Length / 2; }
        }

        public virtual void Draw(DrawingContext dc) {
            
        }

        public Node GetNeighbourNode(Node node) {
            if (node == StartNode) return EndNode;
            if (node == EndNode) return StartNode;
            return null;
        }

        public virtual void UpdateForces(double linkMomentConstant, double defaultSpringLength, double attractionConstant) {
            UpdateAttractionForces(defaultSpringLength, attractionConstant);
            UpdateDiscreteAngleForces(linkMomentConstant);
        }

        public virtual void UpdateDiscreteAngleForces(double linkMomentConstant) {

            if (PreferredAngles.Length == 0) return;

            var node = StartNode;
            var connectedNode = EndNode;

            var vectorToConnectedNode = connectedNode.Pos - node.Pos;
            vectorToConnectedNode.Normalize();
            var rotateClockwiseVector = Vector3D.CrossProduct(vectorToConnectedNode, new Vector3D(0, 0, 1));
            var rotateCounterClockwiseVector = Vector3D.CrossProduct(vectorToConnectedNode, new Vector3D(0, 0, -1));
            double angle0 = Angle0;

            // Try to refactor enum into array of doubles
            double[] preferredAngles = PreferredAngles;
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
            var force = torsionForce * HalfLength; // Force is exerted from the middle of the link

            node.AddForce(ForceType.DiscreteAngles, vector * force);
            connectedNode.AddForce(ForceType.DiscreteAngles, -vector * force);
        }

        public virtual void UpdateAttractionForces(double defaultSpringLength, double attractionConstant) {
            var force = CalcAttractionForce(defaultSpringLength, attractionConstant);
            StartNode.AddForce(ForceType.Attraction, force);
            EndNode.AddForce(ForceType.Attraction, -force);
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

        private double GetDistanceToPoint(Point p) {
            var p1 = StartNode.Pos2D;
            var p2 = EndNode.Pos2D;
            var angle = Vector.AngleBetween(p2 - p1, p - p1);
            return Math.Abs(Math.Sin(angle)*(p - p1).Length);
        }

        /// <summary>
        /// Calculates the attraction force between two connected nodes, using the specified spring length.
        /// </summary>
        /// <param name="a">The node that the force is acting on.</param>
        /// <param name="b">The node creating the force.</param>
        /// <param name="springLength">The length of the spring, in pixels.</param>
        /// <returns>A Vector representing the attraction force.</returns>
        private Vector3D CalcAttractionForce(double springLength, double attractionConstant) {
            if (StartNode == EndNode) return new Vector3D(0, 0, 0);
            var proximity = Math.Max(StartNode.GetDistanceToNode(EndNode), 0);
            //var proximity = CalcDistance(a, b);

            // Hooke's Law: F = -kx
            var force = attractionConstant * (proximity - springLength);
            //var force = ATTRACTION_CONSTANT * (proximity - springLength);
            //var angle = GetBearingAngle(a, b);
            //var angle = Math.Atan((b.Y - a.Y) / (b.X - a.X));

            //return new Vector(force * Math.Cos(angle), force * Math.Sin(angle));
            var vector = EndNode.Pos - StartNode.Pos;
            if (vector.Length > 0) {
                vector.Normalize();
            }
            return vector * force;
        }
    }

    public static class VectorExt {
        private const double DegToRad = Math.PI / 180;

        public static Vector Rotate(this Vector v, double degrees) {
            return v.RotateRadians(degrees * DegToRad);
        }

        public static Vector RotateRadians(this Vector v, double radians) {
            var ca = Math.Cos(radians);
            var sa = Math.Sin(radians);
            return new Vector(ca * v.X - sa * v.Y, sa * v.X + ca * v.Y);
        }
    }
}