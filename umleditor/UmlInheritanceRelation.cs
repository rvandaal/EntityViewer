using System;
using System.Windows;
using System.Windows.Media;
using GraphFramework;

namespace UmlEditor {
    public class UmlInheritanceRelation : UmlRelation {
        private const double ArrowLength = 18;
        private readonly Vector offsetVector = new Vector(0, 0);

        public UmlInheritanceRelation(string preferredAngleString) : base(preferredAngleString) { }

        public override void Draw(DrawingContext dc) {
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
            if (v.Length > 0) {
                //
                // Startpoint that lies on the edge of an UmlClass
                //
                var p1 = StartPoint + v*StartOffset + offsetVector;
                //
                // Endpoint that lies on the edge of an UmlClass
                //
                var p2 = EndPoint - v*EndOffset + offsetVector;

                var v1 = v;
                v1.Normalize();
                var p3 = p2 - v1*Math.Cos(30*Math.PI/180)*ArrowLength;

                var v2 = v*Rotation30Matrix;
                var v3 = v*RotationMin30Matrix;
                v2.Normalize();
                v3.Normalize();
                var p4 = p2 - v2*ArrowLength;
                var p5 = p2 - v3*ArrowLength;

                dc.DrawLine(GetMainLinePen(), p1, p3);
                dc.DrawLine(Utils.DefaultPen, p3, p4);
                dc.DrawLine(Utils.DefaultPen, p4, p2);
                dc.DrawLine(Utils.DefaultPen, p2, p5);
                dc.DrawLine(Utils.DefaultPen, p5, p3);

                base.Draw(dc);
            }
        }

        protected virtual Pen GetMainLinePen() {
            return Utils.DefaultPen;
        }
    }
}
