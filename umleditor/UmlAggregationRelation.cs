using System;
using System.Windows;
using System.Windows.Media;
using GraphFramework;

namespace UmlEditor {
    public class UmlAggregationRelation : UmlRelation {
        private const double ArrowLength = 15;

        public UmlAggregationRelation(string preferredAngleString, string startMultiplicity = "1", string endMultiplicity = "1") : base(preferredAngleString, startMultiplicity, endMultiplicity) { }

        public override void Draw(DrawingContext dc) {
            //
            //         p2
            //         /\
            //        /  \
            //     p4 \  / p5
            //         \/
            //         | p3
            //         |
            //         p1
            //
            Vector v = EndPoint - StartPoint;
            var v1 = v;
            v1.Normalize();
            Vector orthogonalVector = v1.Rotate(90);

            if (v.Length > 0) {
                //
                // Startpoint that lies on the edge of an UmlClass
                //
                var p1 = StartPoint + v * StartOffset;
                //
                // Endpoint that lies on the edge of an UmlClass
                //
                var p2 = EndPoint - v * EndOffset;

                
                var p3 = p2 - 2 * v1 * Math.Cos(30 * Math.PI / 180) * ArrowLength;

                var v2 = v * Rotation30Matrix;
                var v3 = v * RotationMin30Matrix;
                v2.Normalize();
                v3.Normalize();
                var p4 = p2 - v2 * ArrowLength;
                var p5 = p2 - v3 * ArrowLength;

                var segments = new[] {
                                         new LineSegment(p3, true),
                                         new LineSegment(p4, true),
                                         new LineSegment(p2, true),
                                         new LineSegment(p5, true),
                                         new LineSegment(p3, true)
                                     };

                var figure = new PathFigure(p1, segments, true);
                var geo = new PathGeometry(new[] { figure });
                dc.DrawGeometry(GetFillBrush(), Utils.DefaultPen, geo);

                // Multiplicity
                //dc.DrawText(
                //    Global.GetFormattedText("N"),
                //    p1 + v1 * 20 + orthogonalVector * 20
                //);

                //dc.DrawText(
                //    Global.GetFormattedText("1"),
                //    p2 - v1 * 20 + orthogonalVector * 20
                //);

                base.Draw(dc);
            }
        }

        protected virtual Brush GetFillBrush() {
            return null;
        }
    }
}
