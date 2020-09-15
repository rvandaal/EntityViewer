using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;

namespace GraphFramework {
    public class GraphSimulator : DependencyObject {

        public Graph Graph { get; set; }

        #region Node to Node

        public double AttractionConstant {
            get { return (double)GetValue(AttractionConstantProperty); }
            set { SetValue(AttractionConstantProperty, value); }
        }

        public static readonly DependencyProperty AttractionConstantProperty =
            DependencyProperty.Register(
                "AttractionConstant",
                typeof(double),
                typeof(GraphSimulator),
                new FrameworkPropertyMetadata(0.1)
            );

        public double RepulsionConstant {
            get { return (double)GetValue(RepulsionConstantProperty); }
            set { SetValue(RepulsionConstantProperty, value); }
        }

        public static readonly DependencyProperty RepulsionConstantProperty =
            DependencyProperty.Register(
                "RepulsionConstant",
                typeof(double),
                typeof(GraphSimulator),
                new FrameworkPropertyMetadata(500000.0)
            );

        public double DefaultDamping {
            get { return (double)GetValue(DefaultDampingProperty); }
            set { SetValue(DefaultDampingProperty, value); }
        }

        public static readonly DependencyProperty DefaultDampingProperty =
            DependencyProperty.Register(
                "DefaultDamping",
                typeof(double),
                typeof(GraphSimulator),
                new FrameworkPropertyMetadata(0.85)
            );

        public double DefaultSpringLength {
            get { return (double)GetValue(DefaultSpringLengthProperty); }
            set { SetValue(DefaultSpringLengthProperty, value); }
        }

        public static readonly DependencyProperty DefaultSpringLengthProperty =
            DependencyProperty.Register(
                "DefaultSpringLength",
                typeof(double),
                typeof(GraphSimulator),
                new FrameworkPropertyMetadata(1.0)
            );

        public double DefaultRepulsionHorizon {
            get { return (double)GetValue(DefaultRepulsionHorizonProperty); }
            set { SetValue(DefaultRepulsionHorizonProperty, value); }
        }

        public static readonly DependencyProperty DefaultRepulsionHorizonProperty =
            DependencyProperty.Register(
                "DefaultRepulsionHorizon",
                typeof(double),
                typeof(GraphSimulator),
                new FrameworkPropertyMetadata(500.0)
            );

        public double LinkMomentConstant {
            get { return (double)GetValue(LinkMomentConstantProperty); }
            set { SetValue(LinkMomentConstantProperty, value); }
        }

        public static readonly DependencyProperty LinkMomentConstantProperty =
            DependencyProperty.Register(
                "LinkMomentConstant",
                typeof(double),
                typeof(GraphSimulator),
                new FrameworkPropertyMetadata(0.05)
            );

        public double NeighbourConnectorRepulsion {
            get { return (double)GetValue(NeighbourConnectorRepulsionProperty); }
            set { SetValue(NeighbourConnectorRepulsionProperty, value); }
        }

        public static readonly DependencyProperty NeighbourConnectorRepulsionProperty =
            DependencyProperty.Register(
                "NeighbourConnectorRepulsion",
                typeof(double),
                typeof(GraphSimulator),
                new FrameworkPropertyMetadata(0.0)
            );

        public double SnapAngle {
            get { return (double)GetValue(SnapAngleProperty); }
            set { SetValue(SnapAngleProperty, value); }
        }

        public static readonly DependencyProperty SnapAngleProperty =
            DependencyProperty.Register(
                "SnapAngle",
                typeof(double),
                typeof(GraphSimulator),
                new FrameworkPropertyMetadata(0.0)
            );

        public double LinkCrossingConstant {
            get { return (double)GetValue(LinkCrossingConstantProperty); }
            set { SetValue(LinkCrossingConstantProperty, value); }
        }

        public static readonly DependencyProperty LinkCrossingConstantProperty =
            DependencyProperty.Register(
                "LinkCrossingConstant",
                typeof(double),
                typeof(GraphSimulator),
                new FrameworkPropertyMetadata(0.0)
            );

        public double Node2LinkRepulsion {
            get { return (double)GetValue(Node2LinkRepulsionProperty); }
            set { SetValue(Node2LinkRepulsionProperty, value); }
        }

        public static readonly DependencyProperty Node2LinkRepulsionProperty =
            DependencyProperty.Register(
                "Node2LinkRepulsion",
                typeof(double),
                typeof(GraphSimulator),
                new FrameworkPropertyMetadata(0.0)
            );

        #endregion

        public bool CenterNodes {
            get { return (bool)GetValue(CenterNodesProperty); }
            set { SetValue(CenterNodesProperty, value); }
        }
        //
        // CenterNodes werkt nog niet, want als je nodes in center van umlcanvas zet, dan wordt de canvas kleiner, dus de nodes zullen altijd aan de rand van het canvas zitten.
        // Tegelijkertijd wordt het canvas gecentreerd in de viewport; dit conflicteert dus.
        //
        public static readonly DependencyProperty CenterNodesProperty =
            DependencyProperty.Register(
                "CenterNodes",
                typeof(bool),
                typeof(GraphSimulator),
                new FrameworkPropertyMetadata(true)
            );

        public double Simulate(double dt, double viewportWidth, double viewportHeight, bool isSimulating) {
            if (Graph == null) {
                return 0.0;
            }
            // IDEA: use mousewheel to control the dt and therefore the stability of the simulation.
            // Unstable? Scroll the mousewheel (in the good direction)

            UpdateForces(viewportWidth, viewportHeight);
            if (isSimulating) {
                var kineticEnergy = UpdateAccVelPos(dt);
                if (CenterNodes && Graph.CurrentOperation == DragOperation.None) {
                    ApplyOffsetToCenter(viewportWidth, viewportHeight);
                }
                return kineticEnergy;
            }
            return 0.0;
        }

        private void UpdateForces(double viewportWidth, double viewportHeight) {
            var centerX = viewportWidth/2;
            var centerY = viewportHeight/2;

            foreach (var node in Graph.Nodes) {
                //
                // The UpdateNeighbourConnectorRepulsionForces operates in the context of one node,
                // but adds a force to some other node. Although the rest of the updates doesn't influence other nodes,
                // this is better for performance, because we have less loops in loops in loops.
                // So we have to reset before all updates, otherwise we could get: node0 updates a force on node1, after that node1 resets.
                //
                node.ResetForces();
            }

            foreach (var node in Graph.VisibleNodes) {
                // determine repulsion between nodes
                foreach (var otherNode in Graph.VisibleNodes.Where(n => n != node)) {
                    node.AddForce(ForceType.Repulsion, CalcRepulsionForce(node, otherNode));
                }
                foreach (Link link in node.Links.Where(l => l.IsVisible)) {
                    if (link.GetNeighbourNode(node) == null) continue;
                    UpdateNeighbourConnectorRepulsionForces(node, link);
                }
            }

            foreach (Link link in Graph.Links.Where(l => l.IsSimulated && l.IsVisible)) {
                link.UpdateForces(LinkMomentConstant, DefaultSpringLength, AttractionConstant);
                foreach(var node in Graph.VisibleNodes.Where(n => link.StartNode != n && link.EndNode != n)) {
                    UpdateNode2LinkRepulsion(link, node);
                }
            }

            UpdateLink2LinkRepulsionForces(centerX, centerY);            
        }

        private void UpdateNeighbourConnectorRepulsionForces(Node node, Link link) {
            if (NeighbourConnectorRepulsion == 0) return;

            var connectedNode = link.GetNeighbourNode(node);
            if (connectedNode == node) return;
            var vectorToConnectedNode = connectedNode.Pos - node.Pos;
            var vectorToConnectedNode2D = connectedNode.Pos2D - node.Pos2D;

            // TODO zondag:
            // Op het einde van de volgende loop zijn alle link krachten weer 0, terwijl deze
            // aan het begin wel gezet werden (op (-1128,0,0) oid)

            foreach (Link link2 in node.Links.Where(l => l.IsSimulated && l != link)) {
                //Debug.WriteLine("NeighbourConnector repulsion - Node: " + node.Index + ", Link1: " + link.Index + ", Link2: " + link2.Index);
                // Link1 should be repulsed by link2. This is the same as link1 should be attracted by the oppositie of link2 (= -vector of link2)
                // We will consider this opposite vector so add 180 (not -180 because another potential 180 will be subtracted)

                Vector3D vectorLink2 = link2.GetNeighbourNode(node).Pos - node.Pos;
                Vector vectorLink22D = link2.GetNeighbourNode(node).Pos2D - node.Pos2D;
                Vector3D oppositeVectorLink2 = -vectorLink2;
                double angleBetweenLink1AndLink2 = Vector.AngleBetween(vectorToConnectedNode2D, vectorLink22D);

                // Door een crossproduct te gebruiken zoals hier, hoef je geen angles van elkaar af te trekken, want dit geeft altijd
                // problemen. Gebruik gewoon Vector.AngleBetween en cross product om te vector af te leiden.
                Vector3D rotateVector = Vector3D.CrossProduct(oppositeVectorLink2, vectorToConnectedNode);
                rotateVector.Normalize();

                var vector22 = Vector3D.CrossProduct(vectorToConnectedNode, rotateVector);
                vector22.Normalize();

                //---------------------------------------------------------------------
                // Vector is determined, now determine how big the force(.Length) is
                //
                var finalAngle = Math.Max(Math.Abs(angleBetweenLink1AndLink2), 1);

                var torsionForce2 = NeighbourConnectorRepulsion / Math.Pow(finalAngle, 2);
                //
                // Rotation is seen from node, the rotation axis is the node's center. So, the arm is link.Length.
                //
                var force2 = torsionForce2 * link.HalfLength * 2;


                // Coulomb's Law: F = k(Qq/r^2)


                // is niet zo makkelijk; Angle0 is de hoek van startpoint naar endpoint
                // we moeten dus kijken of deze link uitkomt bij de node met het startpoint of endpoint

                var otherNode = link.GetNeighbourNode(node);
                //
                // TODO: This force will be reversed when a isa b isa c isa d because then only a has outgoing relations
                // So in this example, all relations will align.
                // This force will only work for ingoing relations. Like b isa a. c isa a. d isa a.
                //
                var forceVector = vector22 * force2;
                otherNode.AddForce(ForceType.NeighbourConnectorRepulsion, forceVector);

                //Debug.WriteLine("Added force to Node: " + otherNode.Index.ToString() + ", via Link: " + link.Index.ToString() + ", force: (" + forceVector.ToString() + ")");
            }
        }

        private void UpdateLink2LinkRepulsionForces(double centerX, double centerY) {
            if (LinkCrossingConstant < 1e-4) {
                return;
            }
            for (int i = 0; i < Graph.Links.Count - 1; i++) {
                var link1 = Graph.Links[i];
                if (!link1.IsSimulated) continue;
                for (int j = i + 1; j < Graph.Links.Count; j++) {
                    var link2 = Graph.Links[j];
                    if (!link2.IsSimulated) continue;
                    if (
                        link1 != link2 &&
                        AreIntersecting(
                            ProjectPoint(link1.StartNode.Pos, centerX, centerY),
                            ProjectPoint(link1.EndNode.Pos, centerX, centerY),
                            ProjectPoint(link2.StartNode.Pos, centerX, centerY),
                            ProjectPoint(link2.EndNode.Pos, centerX, centerY)
                        )
                    ) {
                        var z1 = (link1.StartNode.Pos.Z + link1.EndNode.Pos.Z) / 2;
                        var z2 = (link2.StartNode.Pos.Z + link2.EndNode.Pos.Z) / 2;
                        var a = LinkCrossingConstant;
                        var v = new Vector3D(0, 0, a);

                        if (z1 < z2) {
                            link1.StartNode.AddForce(ForceType.Link2LinkRepulsion, -v);
                            link1.EndNode.AddForce(ForceType.Link2LinkRepulsion, -v);
                            link2.StartNode.AddForce(ForceType.Link2LinkRepulsion, v);
                            link2.EndNode.AddForce(ForceType.Link2LinkRepulsion, v);
                        } else {
                            link1.StartNode.AddForce(ForceType.Link2LinkRepulsion, v);
                            link1.EndNode.AddForce(ForceType.Link2LinkRepulsion, v);
                            link2.StartNode.AddForce(ForceType.Link2LinkRepulsion, -v);
                            link2.EndNode.AddForce(ForceType.Link2LinkRepulsion, -v);
                        }
                    }
                }
            }
        }

        private void UpdateNode2LinkRepulsion(Link link, Node node) {
            // Compute a force that exists between a link and the closest corner of a node.
            if (Node2LinkRepulsion < 1e-4) return;
            Vector3D direction;
            double distance;
           bool applyForce = GetDistanceFromPointToLine(node, link.StartNode, link.EndNode, out distance, out direction);
            if(applyForce && distance > 0 && distance < DefaultRepulsionHorizon / 6) {
                //var force = -Node2LinkRepulsion / Math.Pow(distance / 10, 2);
                var force = -Node2LinkRepulsion/distance * 10;
                var repulsionVector = direction;
                link.StartNode.AddForce(ForceType.Node2LinkRepulsion, force * repulsionVector);
                link.EndNode.AddForce(ForceType.Node2LinkRepulsion, force * repulsionVector);
            }
        }

        public Point ProjectPoint(Point3D point, double centerX, double centerY) {
            var distanceXToCenter = point.X - centerX;
            var distanceYToCenter = point.Y - centerY;
            distanceXToCenter /= point.Z;
            distanceYToCenter /= point.Z;
            return new Point(centerX + distanceXToCenter, centerY + distanceYToCenter);
        }

        private double UpdateAccVelPos(double deltaTime) {
            double kineticEnergy = 0.0;
            foreach (var node in Graph.SimulatedNodes) {
                node.Acc = node.Force / node.Mass;
                node.Vel = (node.Vel + node.Acc * deltaTime) * GetDefaultDamping(node);
                node.Pos += node.Vel * deltaTime;
                kineticEnergy += 0.5 * node.Mass * node.Vel.LengthSquared;
            }
            //if (kineticEnergy > 10000) {
            //    foreach (var node in Graph.SimulatedNodes) {
            //        node.Vel = new Vector3D(0, 0, 0);
            //    }
            //}
            return kineticEnergy;
        }

        //private void ApplyOffsetToCenter(double viewportWidth, double viewportHeight) {
        //    Point3D middle = new Point3D(0, 0, 0);
        //    var numberOfSimulatedNodes = Graph.SimulatedNodes.Count();
        //    foreach (var node in Graph.VisibleNodes) {
        //        middle.X += node.Pos.X;
        //        middle.Y += node.Pos.Y;
        //        middle.Z += node.Pos.Z;
        //    }
        //    middle.X /= numberOfSimulatedNodes;
        //    middle.Y /= numberOfSimulatedNodes;
        //    middle.Z /= numberOfSimulatedNodes;

        //    foreach (var node in Graph.SimulatedNodes) {
        //        node.Pos = new Point3D(node.Pos.X + viewportWidth / 2 - middle.X, node.Pos.Y + viewportHeight / 2 - middle.Y, Math.Max(1, node.Pos.Z - middle.Z));
        //    }
        //}

        protected virtual void ApplyOffsetToCenter(double viewportWidth, double viewportHeight) {
            Vector3D leftTop = new Vector3D(double.PositiveInfinity, double.PositiveInfinity, 0);
            foreach (var node in Graph.VisibleNodes) {
                leftTop.X = Math.Min(node.Pos.X, leftTop.X);
                leftTop.Y = Math.Min(node.Pos.Y, leftTop.Y);
            }
            //Debug.WriteLine(leftTop.ToString());

            foreach (var node in Graph.SimulatedNodes) {
                node.Pos -= leftTop;
            }
        }

        /// <summary>
        /// Calculates the repulsion force between any two nodes in the diagram space.
        /// </summary>
        /// <param name="a">The node that the force is acting on.</param>
        /// <param name="b">The node creating the force.</param>
        /// <returns>A Vector representing the repulsion force.</returns>
        private Vector3D CalcRepulsionForce(Node a, Node b) {
            if (a == b) return new Vector3D(0, 0, 0);
            var proximity = Math.Max(a.GetDistanceToNode(b), 1);

            if(proximity > GetDefaultRepulsionHorizon(a, b)) {
                return new Vector3D(0,0,0);
            }

            // Coulomb's Law: F = k(Qq/r^2)
            var force = -(GetRepulsionConstant(a, b) / Math.Pow(proximity, 2));
            //var angle = GetBearingAngle(a, b);

            //return new Vector(force * Math.Cos(angle), force * Math.Sin(angle));
            var vector = b.Pos - a.Pos;
            if (vector.Length > 0) {
                vector.Normalize();
            }
            return vector * force;
        }

        ///// <summary>
        ///// Calculates the repulsion force between a node and the left edge in the diagram space.
        ///// </summary>
        ///// <param name="a">The node that the force is acting on.</param>
        ///// <returns>A Vector representing the repulsion force.</returns>
        //private Vector CalcRepulsionForceFromLeftEdge(Node a) {
        //    return (RepulsionConstant / Math.Pow(Math.Max(1, a.Pos.X), 2)) * new Vector(1, 0);
        //}

        ///// <summary>
        ///// Calculates the repulsion force between a node and the left edge in the diagram space.
        ///// </summary>
        ///// <param name="a">The node that the force is acting on.</param>
        ///// <returns>A Vector representing the repulsion force.</returns>
        //private Vector CalcRepulsionForceFromRightEdge(Node a) {
        //    return (RepulsionConstant / Math.Pow(ActualWidth - Math.Max(1, a.Pos.X), 2)) * new Vector(-1, 0);
        //}

        ///// <summary>
        ///// Calculates the repulsion force between a node and the left edge in the diagram space.
        ///// </summary>
        ///// <param name="a">The node that the force is acting on.</param>
        ///// <returns>A Vector representing the repulsion force.</returns>
        //private Vector CalcRepulsionForceFromTopEdge(Node a) {
        //    return (RepulsionConstant / Math.Pow(Math.Max(1, a.Pos.Y), 2)) * new Vector(0, 1);
        //}

        ///// <summary>
        ///// Calculates the repulsion force between a node and the left edge in the diagram space.
        ///// </summary>
        ///// <param name="a">The node that the force is acting on.</param>
        ///// <returns>A Vector representing the repulsion force.</returns>
        //private Vector CalcRepulsionForceFromBottomEdge(Node a) {
        //    return (RepulsionConstant / Math.Pow(ActualHeight - Math.Max(1, a.Pos.Y), 2)) * new Vector(0, -1);
        //}

        private double GetAttractionConstant(Node node1, Node node2) {
            return AttractionConstant;
        }

        private double GetDefaultRepulsionHorizon(Node node1, Node node2) {
            return DefaultRepulsionHorizon;
        }

        private double GetRepulsionConstant(Node node1, Node node2) {
            return RepulsionConstant;
        }

        private double GetDefaultSpringLength(Node node1, Node node2) {
            return DefaultSpringLength;
        }

        private double GetDefaultDamping(Node node) {
            return DefaultDamping;
        }

        /// <summary>
        /// Calculates the distance between two points.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <returns>The pixel distance between the two points.</returns>
        public static double CalcDistance(Node a, Node b) {
            //double xDist = (a.Pos.X - b.Pos.X);
            //double yDist = (a.Pos.Y - b.Pos.Y);
            //return Math.Sqrt(Math.Pow(xDist, 2) + Math.Pow(yDist, 2));
            return (a.Pos - b.Pos).Length;
        }

        /// <summary>
        /// Calculates the distance between two points.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <returns>The pixel distance between the two points.</returns>
        public static double CalcDistance(Point3D a, Node b) {
            //double xDist = (a.X - b.Pos.X);
            //double yDist = (a.Y - b.Pos.Y);
            //double zDist = (a.Z - b.Pos.Z);
            //return Math.Sqrt(Math.Pow(xDist, 2) + Math.Pow(yDist, 2) + Math.Pow(zDist, 2));
            return (b.Pos - a).Length;
        }

        private bool AreIntersecting(Point p1, Point p2, Point p3, Point p4) {
            var dx12 = p2.X - p1.X;
            var dy12 = p2.Y - p1.Y;
            var dx34 = p4.X - p3.X;
            var dy34 = p4.Y - p3.Y;
            var denominator = dy12*dx34 - dx12*dy34;
            if(denominator == 0) {
                return false;
            }
            var t1 = ((p1.X - p3.X)*dy34 + (p3.Y - p1.Y)*dx34)/denominator;
            var t2 = -((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)/denominator;
            //var intersection = new Point(p1.X + dx12*t1, p1.Y + dy12*t1);
            return t1 >= 0.1 && t1 <= 0.9 && t2 >= 0.1 && t2 <= 0.9;
        }

        private static bool GetDistanceFromPointToLine(Node p, Node l1, Node l2, out double distance, out Vector3D direction) {
            // http://www.sinclair.edu/centers/mathlab/pub/findyourcourse/worksheets/Calculus/VectorMethodsIn3-Space.pdf
            var pl1 = p.Pos - l1.Pos;
            var pl2 = l2.Pos - p.Pos;
            var line = l2.Pos - l1.Pos;
            var pl1d = p.Pos2D - l1.Pos2D;
            var pl2d = l2.Pos2D - p.Pos2D;
            var lined = l2.Pos2D - l1.Pos2D;
            if(Vector.AngleBetween(pl1d, lined) <= 90 && Vector.AngleBetween(pl2d, lined) <= 90) {
                var crossProduct = Vector3D.CrossProduct(pl1, line);
                var crossProduct2 = Vector3D.CrossProduct(line, crossProduct);
                direction = crossProduct2;
                direction.Normalize();
                distance = crossProduct.Length/line.Length;
                return true;
            }
            distance = double.NaN;
            direction = new Vector3D(0,0,0);
            return false;
        }

        private void UnusedMethodToComputeSnappingForces() {
            //double angleWithinSnapRange = double.NaN;
            //bool rotateClockwiseWithinSnapRange = false;

            //if (angle0 > 0 && angle0 <= SnapAngle || angle0 > -180 && angle0 <= -180 + SnapAngle) {
            //    angleWithinSnapRange = angle0;
            //    rotateClockwiseWithinSnapRange = true;
            //} else if (angle0 > 90 && angle0 <= 90 + SnapAngle || angle0 > -90 && angle0 <= -90 + SnapAngle) {
            //    angleWithinSnapRange = angle90;
            //    rotateClockwiseWithinSnapRange = true;
            //} else if (angle0 > 180 - SnapAngle && angle0 <= 180 || angle0 > -SnapAngle && angle0 <= 0) {
            //    angleWithinSnapRange = angle0;
            //    rotateClockwiseWithinSnapRange = false;
            //} else if (angle0 > 90 - SnapAngle && angle0 <= 90 || angle0 > -90 - SnapAngle && angle0 <= -90) {
            //    angleWithinSnapRange = angle90;
            //    rotateClockwiseWithinSnapRange = false;
            //}
        }

        private void UnusedMethodToComputeEdgeForces() {
            //determine repulsion from edges of the screen
            //node.Force += CalcRepulsionForceFromLeftEdge(node);
            //node.Force += CalcRepulsionForceFromRightEdge(node);
            //node.Force += CalcRepulsionForceFromTopEdge(node);
            //node.Force += CalcRepulsionForceFromBottomEdge(node);
            //double absoluteForce = node.Force.Length;
            //if (absoluteForce > 1000) {
            //    node.Force = node.Force * 1000 / absoluteForce;
            //}
        }
    }
}
