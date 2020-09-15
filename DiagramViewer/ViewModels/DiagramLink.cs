using System;
using System.Windows;
using System.Windows.Media;
using DiagramViewer.Utilities;

namespace DiagramViewer.ViewModels {
    public class DiagramLink : ViewModelBase {

        public DiagramLink(DiagramNode startNode, DiagramNode endNode) {
            StartNode = startNode;
            EndNode = endNode;
        }

        public DiagramNode StartNode { get; private set; }
        public DiagramNode EndNode { get; private set; }

        public DiagramNode GetNeighbourNode(DiagramNode diagramNode) {
            if (diagramNode == StartNode) return EndNode;
            if (diagramNode == EndNode) return StartNode;
            return null;
        }

        public virtual string Label { get { return null; } }

        public bool IsVisible { get; set; }

        public double[] PreferredAngles { get; set; }

        public double BendingOffset { get; set; }

        public Point StartPoint {
            get {
                if (StartNode != null) {
                    return StartNode.Pos;
                }
                return new Point(0, 0);
            }
        }

        public Point EndPoint {
            get {
                if (EndNode != null) {
                    return EndNode.Pos;
                }
                return new Point(0, 0);
            }
        }

        public Point BendingPoint {
            get {
                if (DoubleUtility.AreClose(BendingOffset, 0)) {
                    return StartPoint + HalfLength * NormalizedVector;
                }
                return StartPoint + HalfLength * NormalizedVector + OrthogonalVector * BendingOffset;
            }
        }

        public Vector NormalizedVector {
            get {
                Vector vector = EndPoint - StartPoint;
                vector.Normalize();
                return vector;
            }
        }

        public Vector OrthogonalVector {
            get { return NormalizedVector.Rotate(90); }
        }

        public Point StartConnectorPoint { get; set; }
        public Point EndConnectorPoint { get; set; }

        public double StartOffset { get; set; }
        public double EndOffset { get; set; }

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
            //
            get { return Vector.AngleBetween((EndPoint - StartPoint), -Utils.Angle90Vector); }
        }

        public double HalfLength {
            get { return (EndNode.Pos - StartNode.Pos).Length / 2; }
        }

        public double LabelWidth { get; set; }

        public virtual void Draw(DrawingContext dc) {
            UpdateCurveParameters();
        }
        
        protected void GetTextPositionOnCurve(
            FormattedText formattedText, 
            double offset, 
            double absOffset, 
            out Point textpoint, 
            out double angle
        ) {
            var textwidth = formattedText.Width;
            var textheight = formattedText.Height;
            var point = GetPointOnVisibleCurve(offset);
            var directionVector = GetDirectionVectorOnVisibleCurve(offset);
            var textDirection = directionVector;
            var orthogonalDirectionCurve = directionVector.Rotate(90);
            angle = Vector.AngleBetween(Utils.Angle0Vector, directionVector);
            Vector verticalOffset;
            if (angle > 90 || -90 > angle) {
                //
                // Angle points to topleft or bottomright quadrant.
                //
                angle += 180;
                textDirection = -textDirection;
                if (BendingOffset > 0) {
                    verticalOffset = orthogonalDirectionCurve * textheight;
                } else {
                    verticalOffset = new Vector(0, 0);
                }
            } else {
                if (BendingOffset < 0) {
                    verticalOffset = orthogonalDirectionCurve * textheight;
                } else {
                    verticalOffset = new Vector(0, 0);
                }
            }

            if (directionVector.X > 0 && BendingOffset < 0) {
                verticalOffset -= 2 * orthogonalDirectionCurve * textheight;
            }

            textpoint = point - textDirection * textwidth / 2 + verticalOffset + absOffset * directionVector;
        }

        private double ax;
        private double ay;
        private double bx;
        private double by;
        private double cx;
        private double cy;

        private void UpdateCurveParameters() {
            var p0 = StartPoint;
            var p1 = BendingPoint;
            var p2 = EndPoint;

            ax = p2.X + p0.X - 2 * p1.X;
            ay = p2.Y + p0.Y - 2 * p1.Y;
            bx = 2 * p1.X - 2 * p0.X;
            by = 2 * p1.Y - 2 * p0.Y;
            cx = p0.X;
            cy = p0.Y;

            if (StartNode != null) {
                double offset0, offset1;
                if (GetCurveOffsetForNode(StartNode, out offset0, out offset1)) {
                    if (double.IsNaN(offset0) && !double.IsNaN(offset1)) {
                        StartOffset = offset1;
                    } else if (double.IsNaN(offset1) && !double.IsNaN(offset0)) {
                        StartOffset = offset0;
                    } else {
                        //throw new Exception("Zero or two intersections found between relation and class");
                        StartOffset = Math.Min(offset0, offset1);
                    }
                }
            }

            if (EndNode != null) {
                double offset0, offset1;
                if (GetCurveOffsetForNode(EndNode, out offset0, out offset1)) {
                    if (double.IsNaN(offset0) && !double.IsNaN(offset1)) {
                        EndOffset = offset1;
                    } else if (double.IsNaN(offset1) && !double.IsNaN(offset0)) {
                        EndOffset = offset0;
                    } else {
                        //throw new Exception("Zero or two intersections found between relation and class");
                        EndOffset = Math.Max(offset0, offset1);
                    }
                }
            }

            StartConnectorPoint = GetPointOnVisibleCurve(0.0);
            EndConnectorPoint = GetPointOnVisibleCurve(1.0);
        }

        protected Point GetPointOnVisibleCurve(double t) {
            // t should be between 0 and 1
            t = StartOffset + (EndOffset - StartOffset) * t;
            return GetPointOnCurve(t);
        }

        protected Point GetPointOnCurve(double t) {
            // t should be between 0 and 1
            return new Point(t * t * ax + t * bx + cx, t * t * ay + t * by + cy);
        }

        protected Vector GetDirectionVectorOnVisibleCurve(double offset) {
            offset = StartOffset + (EndOffset - StartOffset) * offset;
            return GetDirectionVectorOnCurve(offset);
        }

        protected Vector GetDirectionVectorOnCurve(double offset) {
            var vector = 2 * (1 - offset) * (BendingPoint - StartPoint) + 2 * offset * (EndPoint - BendingPoint); // Derivative formula from wiki
            vector.Normalize();
            return vector;
        }

        protected bool GetCurveOffsetForNode(DiagramNode dn, out double o0, out double o1) {
            double offset0;
            double offset1;
            double x0;
            double x1;
            double y0;
            double y1;
            o0 = double.NaN;
            o1 = double.NaN;
            bool r = false;
            if (SolveCurveOffsetForY(dn.TopLeft.Y, out offset0, out offset1, out x0, out x1)) {
                if (IsBetweenZeroAndOne(offset0) && IsBetween(x0, dn.TopLeft.X, dn.TopRight.X)) {
                    o0 = offset0;
                    r = true;
                }
                if (IsBetweenZeroAndOne(offset1) && IsBetween(x1, dn.TopLeft.X, dn.TopRight.X)) {
                    o1 = offset1;
                    r = true;
                }
                if (r) return true;
            }
            if (SolveCurveOffsetForY(dn.BottomLeft.Y, out offset0, out offset1, out x0, out x1)) {
                if (IsBetweenZeroAndOne(offset0) && IsBetween(x0, dn.TopLeft.X, dn.TopRight.X)) {
                    o0 = offset0;
                    r = true;
                }
                if (IsBetweenZeroAndOne(offset1) && IsBetween(x1, dn.TopLeft.X, dn.TopRight.X)) {
                    o1 = offset1;
                    r = true;
                }
                if (r) return true;
            }
            if (SolveCurveOffsetForX(dn.TopLeft.X, out offset0, out offset1, out y0, out y1)) {
                if (IsBetweenZeroAndOne(offset0) && IsBetween(y0, dn.TopLeft.Y, dn.BottomLeft.Y)) {
                    o0 = offset0;
                    r = true;
                }
                if (IsBetweenZeroAndOne(offset1) && IsBetween(y1, dn.TopLeft.Y, dn.BottomLeft.Y)) {
                    o1 = offset1;
                    r = true;
                }
                if (r) return true;
            }
            if (SolveCurveOffsetForX(dn.TopRight.X, out offset0, out offset1, out x0, out x1)) {
                if (IsBetweenZeroAndOne(offset0) && IsBetween(y0, dn.TopLeft.Y, dn.BottomLeft.Y)) {
                    o0 = offset0;
                    r = true;
                }
                if (IsBetweenZeroAndOne(offset1) && IsBetween(y1, dn.TopLeft.Y, dn.BottomLeft.Y)) {
                    o1 = offset1;
                    r = true;
                }
                if (r) return true;
            }
            return false;
        }

        private bool SolveCurveOffsetForX(double x, out double offset0, out double offset1, out double y0, out double y1) {
            return SolveCurveOffset(x, ax, bx, cx, ay, by, cy, out offset0, out offset1, out y0, out y1);
        }

        private bool SolveCurveOffsetForY(double y, out double offset0, out double offset1, out double x0, out double x1) {
            return SolveCurveOffset(y, ay, by, cy, ax, bx, cx, out offset0, out offset1, out x0, out x1);
        }

        private static bool SolveCurveOffset(
            double xy,
            double axy, double bxy, double cxy,
            double ayx, double byx, double cyx,
            out double offset0, out double offset1,
            out double yx0, out double yx1
        ) {
            offset0 = double.NaN;
            offset1 = double.NaN;
            yx0 = double.NaN;
            yx1 = double.NaN;

            if (Math.Abs(axy) < 0.001) {
                // Curve is a line
                if (Math.Abs(bxy) < 0.001) {
                    return false;
                }
                offset0 = (xy - cxy) / bxy;
                yx0 = ayx * offset0 * offset0 + byx * offset0 + cyx;
                return true;
            }

            var d = bxy * bxy - 4 * axy * (cxy - xy);
            if (d < 0) {
                return false;
            }
            d = Math.Sqrt(d);
            offset0 = (-bxy - d) / 2 / axy;
            offset1 = (-bxy + d) / 2 / axy;
            yx0 = offset0 * offset0 * ayx + offset0 * byx + cyx;
            yx1 = offset1 * offset1 * ayx + offset1 * byx + cyx;
            return true;
        }

        private static bool IsBetweenZeroAndOne(double d) {
            return IsBetween(d, 0, 1);
        }

        private static bool IsBetween(double d, double min, double max) {
            return min <= d && d <= max;
        }
    }
}
