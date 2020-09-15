using System.Windows;
using System.Windows.Media;
using GraphFramework;

namespace UmlEditor {
    public class UmlAssociationRelation : UmlRelation {

        private const double ArrowLength = 16;

        public UmlAssociationRelation(string preferredAngleString) : base(preferredAngleString) { }

        public override void Draw(DrawingContext dc) {
            Vector v = EndPoint - StartPoint;
            var v8 = v;
            v8.Normalize();
            Vector orthogonalVector = v8.Rotate(90);
            if (v.Length > 0) {
                var p1 = StartPoint + v*StartOffset;
                var p2 = EndPoint - v*EndOffset;

                var v5 = p2 - p1;
                var p5 = p1 + v5/2 + orthogonalVector*30;

                var v1 = v*Rotation30Matrix;
                var v2 = v*RotationMin30Matrix;
                v1.Normalize();
                v2.Normalize();
                var p3 = p2 - v1*ArrowLength;
                var p4 = p2 - v2*ArrowLength;

                dc.DrawLine(GetMainLinePen(), p1, p2);
                dc.DrawLine(Utils.DefaultPen, p2, p3);
                dc.DrawLine(Utils.DefaultPen, p2, p4);

                // https://books.google.nl/books?id=7MtkGjIgOxkC&pg=PA135&lpg=PA135&dq=drawingcontext+drawgeometry+bezier+wpf&source=bl&ots=TApW1D5bmd&sig=vlS5sD3lPpH3OfQfTeaRa3bsLxI&hl=nl&sa=X&ved=0CFcQ6AEwBmoVChMI0qXZ4YGSyAIV5afbCh1Jfgzd#v=onepage&q=drawingcontext%20drawgeometry%20bezier%20wpf&f=false

                //PathGeometry geo = new PathGeometry();
                //PathFigure figure = new PathFigure();
                //figure.StartPoint = p1;
                //PolyQuadraticBezierSegment bezier = new PolyQuadraticBezierSegment();
                //bezier.Points.Add(p5);
                //bezier.Points.Add(p2);
                //figure.Segments.Add(bezier);
                //geo.Figures.Add(figure);

                //dc.DrawGeometry(null, GetMainLinePen(), geo);


            }
            base.Draw(dc);
        }

        protected virtual Pen GetMainLinePen() {
            return Utils.DefaultPen;
        }
    }
}
