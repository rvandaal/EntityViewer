using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace GraphFramework {
    public class Rectangle : Shape {

        private bool sizeChangeInitiatedBySize2D;

        public Rectangle(Graph graph) : base(graph) {

        }

        private Size size;
        public Size Size {
            get { return size; }
            set {
                if (size != value) {
                    size = value;
                    OnSizeChanged();
                }
            }
        }

        private Size size2D;
        public Size Size2D {
            get { return size2D; }
            set {
                if (!DoubleUtility.AreClose(Size2D, value)) {
                    size2D = value;
                    sizeChangeInitiatedBySize2D = true;
                    Size = Graph.UnprojectSize(value, Pos.Z);
                    sizeChangeInitiatedBySize2D = false;
                }
            }
        }

        public override double Mass {
            get { return Math.Max(1, Size.Width * Size.Height / 1000); }
        }

        public override bool ContainsPoint2D(Point point) {
            return point.X >= TopLeft2D.X &&
                   point.X <= BottomRight2D.X &&
                   point.Y >= TopLeft2D.Y &&
                   point.Y <= BottomRight2D.Y;
        }

        public override double GetDistanceToNode(Node node) {
            return Math.Min(
                Math.Min(
                    Math.Min(
                        node.GetDistanceToPoint(TopLeft),
                        node.GetDistanceToPoint(TopRight)
                    ),
                    node.GetDistanceToPoint(BottomRight)
                ),
                node.GetDistanceToPoint(BottomLeft)
            );
        }

        public override double GetDistanceToPoint(Point3D point) {

            // Corners first, biggest area
            if (point.X < TopLeft.X && point.Y < TopLeft.Y) {
                return GetDistanceBetweenPoints(TopLeft, point);
            }
            if (point.X > TopRight.X && point.Y < TopRight.Y) {
                return GetDistanceBetweenPoints(TopRight, point);
            }
            if (point.X > BottomRight.X && point.Y > BottomRight.Y) {
                return GetDistanceBetweenPoints(BottomRight, point);
            }
            if (point.X < BottomLeft.X && point.Y > BottomLeft.Y) {
                return GetDistanceBetweenPoints(BottomLeft, point);
            }

            // Edges
            if (point.X < TopLeft.X) {
                return TopLeft.X - point.X;
            }
            if (point.Y < TopLeft.Y) {
                return TopLeft.Y - point.Y;
            }
            if (point.X > TopRight.X) {
                return point.X - TopRight.X;
            }
            if (point.Y > BottomLeft.Y) {
                return point.Y - BottomLeft.Y;
            }
            return 0;
        }

        public override void Draw(DrawingContext dc) {
            dc.DrawRectangle(Utils.NodeBrush, null, new Rect(TopLeft2D, BottomRight2D));
        }

        protected override void OnPosChanged() {
            base.OnPosChanged();
            UpdateProjectedCornerPositions(false);
        }

        protected virtual void OnSizeChanged() {
            UpdateProjectedCornerPositions(true);
        }

        private void UpdateProjectedCornerPositions(bool updateSize) {
            var left = Pos.X - Size.Width / 2;
            var right = Pos.X + Size.Width / 2;
            var top = Pos.Y - Size.Height / 2;
            var bottom = Pos.Y + Size.Height / 2;

            TopLeft = new Point3D(left, top, Pos.Z);
            TopRight = new Point3D(right, top, Pos.Z);
            BottomRight = new Point3D(right, bottom, Pos.Z);
            BottomLeft = new Point3D(left, bottom, Pos.Z);

            TopLeft2D = Graph.ProjectPoint(TopLeft);
            TopRight2D = Graph.ProjectPoint(TopRight);
            BottomRight2D = Graph.ProjectPoint(BottomRight);
            BottomLeft2D = Graph.ProjectPoint(BottomLeft);
            //
            // If the Z coordinate was negative, then TopRight.2D.X < TopLeft2D.X and the same for the Y.
            //
            if (updateSize && !sizeChangeInitiatedBySize2D) {
                Size2D = new Size(Math.Abs(TopRight2D.X - TopLeft2D.X), Math.Abs(BottomLeft2D.Y - TopLeft2D.Y));
            }
            //Debug.WriteLine("Size: " + Size2D.ToString() );
        }

        public Point3D TopLeft { get; private set; }
        public Point3D TopRight { get; private set; }
        public Point3D BottomRight { get; private set; }
        public Point3D BottomLeft { get; private set; }

        public Point TopLeft2D { get; private set; }
        public Point TopRight2D { get; private set; }
        public Point BottomRight2D { get; private set; }
        public Point BottomLeft2D { get; private set; }

        public override double GetDistanceOfConnectorToEdge(Connector connector) {
            var bottomRightCornerAngle = Vector.AngleBetween(Utils.Angle0Vector, BottomRight2D - Pos2D);
            var connectorAngle = connector.Angle0;
            if (Math.Abs(connectorAngle) <= bottomRightCornerAngle || Math.Abs(connectorAngle) >= 180 - bottomRightCornerAngle) {
                return Size2D.Width / 2 / Math.Abs(Math.Cos(connectorAngle / 180 * Math.PI));
            }
            return Size2D.Height / 2 / Math.Sin(Math.Abs(connectorAngle / 180 * Math.PI));
        }
    }
}
