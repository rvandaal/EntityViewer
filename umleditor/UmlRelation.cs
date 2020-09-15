using System;
using System.Windows;
using System.Windows.Media;
using GraphFramework;

namespace UmlEditor {
    public class UmlRelation : Connector {

        private Vector horizontalUnitVector = new Vector(1,0);

        private readonly double halfsqrt2 = Math.Sqrt(2) / 2;
        private readonly double halfsqrt3 = Math.Sqrt(3) / 2;
        protected Matrix Rotation45Matrix;
        protected Matrix RotationMin45Matrix;
        protected Matrix Rotation30Matrix;
        protected Matrix RotationMin30Matrix;

        public string StartMultiplicity { get; set; }
        public string EndMultiplicity { get; set; }

        public UmlRelation(string preferredAngleString, string startMultiplicity = "1", string endMultiplicity = "1")
            : base(preferredAngleString) {
            Rotation45Matrix = new Matrix(halfsqrt2, -halfsqrt2, halfsqrt2, halfsqrt2, 0, 0);
            RotationMin45Matrix = new Matrix(halfsqrt2, halfsqrt2, -halfsqrt2, halfsqrt2, 0, 0);
            Rotation30Matrix = new Matrix(halfsqrt3, -0.5, 0.5, halfsqrt3, 0, 0);
            RotationMin30Matrix = new Matrix(halfsqrt3, 0.5, -0.5, halfsqrt3, 0, 0);
            StartMultiplicity = startMultiplicity;
            EndMultiplicity = endMultiplicity;
        }

        public override void Draw(DrawingContext dc) {
            //if (!string.IsNullOrWhiteSpace(Label)) {
                Vector v = EndPoint - StartPoint;
                var v1 = v;
                v1.Normalize();
                Vector orthogonalVector = v1.Rotate(90);

                if (v.Length > 0) {

                    FormattedText text = null;
                    FormattedText startMultiplictyText = null;
                    FormattedText endMultiplictyText = null;
                    if(!string.IsNullOrEmpty(Label)) {
                        text = Global.GetFormattedText(Label);
                    }
                    if(!string.IsNullOrEmpty(StartMultiplicity) && (StartMultiplicity == "N" || EndMultiplicity == "N")) {
                        startMultiplictyText = Global.GetFormattedText(StartMultiplicity);
                    }
                    if (!string.IsNullOrEmpty(EndMultiplicity) && (StartMultiplicity == "N" || EndMultiplicity == "N")) {
                        endMultiplictyText = Global.GetFormattedText(EndMultiplicity);
                    }
                    //
                    // Startpoint that lies on the edge of an UmlClass
                    //
                    var p1 = StartPoint + v*StartOffset;
                    //
                    // Endpoint that lies on the edge of an UmlClass
                    //
                    var p2 = EndPoint - v*EndOffset;

                    var v2 = p2 - p1;

                    var p3 = p1 + v2/2;
                    Vector verticalOffset = orthogonalVector*5;
                    Vector horizontalOffset = new Vector(0, 0);
                    Vector horizontalOffsetStartMultiplicity = new Vector(0, 0);
                    Vector horizontalOffsetEndMultiplicity = new Vector(0, 0);
                    if (text != null) {
                        horizontalOffset = -v1*text.Width/2;
                    }
                    if(startMultiplictyText != null) {
                        horizontalOffsetStartMultiplicity = -v1 * startMultiplictyText.Width/2;
                    }
                    if (endMultiplictyText != null) {
                        horizontalOffsetEndMultiplicity = -v1 * endMultiplictyText.Width / 2;
                    }
                    double angle = Vector.AngleBetween(horizontalUnitVector, v2);
                    if (angle > 92 || angle < -92) {
                        angle += 180;
                        verticalOffset = -verticalOffset;
                        horizontalOffset = -horizontalOffset;
                        horizontalOffsetStartMultiplicity = -horizontalOffsetStartMultiplicity;
                        horizontalOffsetEndMultiplicity = -horizontalOffsetEndMultiplicity;
                    }
                    Vector offset = horizontalOffset + verticalOffset;
                    Vector offsetStartMultiplicity = horizontalOffsetStartMultiplicity + verticalOffset;
                    Vector offsetEndMultiplicity = horizontalOffsetEndMultiplicity + verticalOffset;
                    Point p4 = p3 + offset;

                    if (text != null) {
                        dc.PushTransform(new RotateTransform(angle, p4.X, p4.Y));
                        dc.DrawText(
                            text,
                            p4
                        );

                        dc.Pop();
                    }

                    Point p5 = p1 + v1 * 20 + offsetStartMultiplicity;
                    Point p6 = p2 - v1 * 20 + offsetEndMultiplicity;

                    if (startMultiplictyText != null) {
                        dc.PushTransform(new RotateTransform(angle, p5.X, p5.Y));
                        dc.DrawText(
                            Global.GetFormattedText(StartMultiplicity),
                            p5
                            );
                        dc.Pop();
                    }

                    if (endMultiplictyText != null) {
                        dc.PushTransform(new RotateTransform(angle, p6.X, p6.Y));
                        dc.DrawText(
                            Global.GetFormattedText(EndMultiplicity),
                            p6
                            );
                        dc.Pop();
                    }

                    base.Draw(dc);
                }
            //}
            base.Draw(dc);
        }
    }
}
