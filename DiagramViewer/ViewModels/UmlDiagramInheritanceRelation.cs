using System;
using System.Windows;
using System.Windows.Media;
using DiagramViewer.Models;
using DiagramViewer.Utilities;

namespace DiagramViewer.ViewModels {
    public class UmlDiagramInheritanceRelation : UmlDiagramRelation {
        private const double ArrowLength = 18;

        public UmlDiagramInheritanceRelation(
            UmlRelation umlRelation,
            UmlDiagramClass startUmlDiagramClass,
            UmlDiagramClass endUmlDiagramClass
        ) : base(umlRelation, startUmlDiagramClass, endUmlDiagramClass) {
            PreferredAngles = new double[] { 90 };
        }

        public override void Draw(DrawingContext dc) {
            if (!IsVisible) return;
            base.Draw(dc);
            //
            //         p2
            //         /\
            //        /  \
            //     p4 -p3- p5
            //         |
            //         |
            //         p1
            //
            Vector v = EndPoint - StartPoint;
            var vn = v;
            vn.Normalize();
            if (v.Length > 0) {
                //
                // Startpoint that lies on the edge of an UmlDiagramClass
                //
                var p1 = StartConnectorPoint;
                //
                // Endpoint that lies on the edge of an UmlDiagramClass
                //
                var p2 = EndConnectorPoint;

                Vector endApproachVector = !DoubleUtility.AreClose(0.0, BendingOffset) ? GetDirectionVectorOnCurve(EndOffset) : vn;
                var v1 = endApproachVector * Utils.Rotation30Matrix;
                var v2 = endApproachVector * Utils.RotationMin30Matrix;

                v1.Normalize();
                v2.Normalize();

                var p3 = p2 - endApproachVector * Math.Cos(30 * Math.PI / 180) * ArrowLength;

                var p4 = p2 - v1*ArrowLength;
                var p5 = p2 - v2*ArrowLength;

                PathGeometry geo = new PathGeometry();
                PathFigure figure = new PathFigure();
                figure.StartPoint = StartPoint;
                if (!DoubleUtility.AreClose(0.0, BendingOffset)) {
                    PolyQuadraticBezierSegment bezier = new PolyQuadraticBezierSegment();
                    bezier.Points.Add(BendingPoint);
                    bezier.Points.Add(EndPoint);
                    figure.Segments.Add(bezier);
                } else {
                    LineSegment lineSegment = new LineSegment(EndPoint, true);
                    figure.Segments.Add(lineSegment);
                }

                PathGeometry geo2 = new PathGeometry();
                PathFigure arrowFigure = new PathFigure();
                arrowFigure.StartPoint = p2;
                PolyLineSegment polyLineSegment = new PolyLineSegment(new[] { p4, p5, p2 }, true);
                
                arrowFigure.Segments.Add(polyLineSegment);

                geo.Figures.Add(figure);
                geo2.Figures.Add(arrowFigure);
                dc.DrawGeometry(null, GetMainLinePen(), geo);
                dc.DrawGeometry(Utils.DefaultBackgroundBrush, Utils.DefaultPen, geo2);
            }
        }

        protected virtual Pen GetMainLinePen() {
            return Utils.DefaultPen;
        }
    }
}
