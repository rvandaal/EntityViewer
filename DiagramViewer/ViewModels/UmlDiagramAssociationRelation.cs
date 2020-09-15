using System.Windows;
using System.Windows.Media;
using DiagramViewer.Models;
using DiagramViewer.Utilities;
using Mono.Cecil;

namespace DiagramViewer.ViewModels {
    public class UmlDiagramAssociationRelation : UmlDiagramRelation {

        private const double ArrowLength = 16;

        public UmlDiagramAssociationRelation(
            UmlRelation umlRelation, 
            UmlDiagramClass startUmlDiagramClass, 
            UmlDiagramClass endUmlDiagramClass
        ) : base(umlRelation, startUmlDiagramClass, endUmlDiagramClass) {
            PreferredAngles = new double[] {  };
        }

        public MemberReference MemberReference {
            get { return ((UmlAssociationRelation)Relation).MemberReference; }
        }

        public override void Draw(DrawingContext dc) {
            if (!IsVisible) return;
            //
            // https://en.wikipedia.org/wiki/B%C3%A9zier_curve
            //
            base.Draw(dc);

            Vector vector = EndPoint - StartPoint;

            if (vector.Length > 0) {

                // Bezier curve defined by StartPoint, EndPoint and BendingOffset
                // Drawing the curve is no problem, determining the EndConnectorPoint is more difficult.

                Vector endApproachVector = !DoubleUtility.AreClose(0.0, BendingOffset) ? GetDirectionVectorOnCurve(EndOffset) : vector;

                var v1 = endApproachVector * Utils.Rotation30Matrix;
                var v2 = endApproachVector * Utils.RotationMin30Matrix;

                v1.Normalize();
                v2.Normalize();
                var p3 = EndConnectorPoint - v1 * ArrowLength;
                var p4 = EndConnectorPoint - v2 * ArrowLength;

                dc.DrawLine(Utils.DefaultPen, EndConnectorPoint, p3);
                dc.DrawLine(Utils.DefaultPen, EndConnectorPoint, p4);

                //// https://books.google.nl/books?id=7MtkGjIgOxkC&pg=PA135&lpg=PA135&dq=drawingcontext+drawgeometry+bezier+wpf&source=bl&ots=TApW1D5bmd&sig=vlS5sD3lPpH3OfQfTeaRa3bsLxI&hl=nl&sa=X&ved=0CFcQ6AEwBmoVChMI0qXZ4YGSyAIV5afbCh1Jfgzd#v=onepage&q=drawingcontext%20drawgeometry%20bezier%20wpf&f=false

                if (!DoubleUtility.AreClose(0.0, BendingOffset)) {
                    PathGeometry geo = new PathGeometry();
                    PathFigure figure = new PathFigure();
                    figure.StartPoint = StartPoint;
                    PolyQuadraticBezierSegment bezier = new PolyQuadraticBezierSegment();
                    bezier.Points.Add(BendingPoint);
                    bezier.Points.Add(EndPoint);
                    figure.Segments.Add(bezier);
                    geo.Figures.Add(figure);

                    dc.DrawGeometry(null, GetMainLinePen(), geo);
                } else {
                    dc.DrawLine(GetMainLinePen(), StartPoint, EndPoint);
                }
            }
        }

        protected virtual Pen GetMainLinePen() {
            return Utils.DefaultPen;
        }
    }
}
