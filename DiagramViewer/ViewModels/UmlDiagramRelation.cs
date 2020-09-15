using System.Windows;
using System.Windows.Media;
using DiagramViewer.Models;
using DiagramViewer.Utilities;

namespace DiagramViewer.ViewModels {
    public class UmlDiagramRelation : DiagramLink {
        internal UmlRelation Relation;
        public UmlDiagramClass StartClass { get { return (UmlDiagramClass)StartNode; } }
        public UmlDiagramClass EndClass { get { return (UmlDiagramClass)EndNode; } }

        public string StartMultiplicity { get { return Relation.StartMultiplicity; } }
        public string EndMultiplicity { get { return Relation.EndMultiplicity; } }

        public override string Label {
            get {
                return Relation.Label;
            }
        }

        public UmlDiagramRelation(UmlRelation umlRelation, UmlDiagramClass startUmlDiagramClass, UmlDiagramClass endUmlDiagramClass)
            : base(startUmlDiagramClass, endUmlDiagramClass) {
            Relation = umlRelation;

            IsVisible = true;
        }

        public override void Draw(DrawingContext dc) {
            base.Draw(dc);

            Vector v = EndPoint - StartPoint;
            var v1 = v;
            v1.Normalize();

            LabelWidth = 0;

            if (v.Length > 0) {
                if (Label != null) {
                    var text = Utils.GetFormattedText(Label);
                    LabelWidth = text.Width;
                    Point p4;
                    double angle;
                    GetTextPositionOnCurve(text, 0.5, 0.0, out p4, out angle);
                    dc.PushTransform(new RotateTransform(angle, p4.X, p4.Y));
                    dc.DrawText(
                        text,
                        p4
                    );
                    dc.Pop();
                }

                if (!string.IsNullOrEmpty(StartMultiplicity) && (StartMultiplicity == "N" || EndMultiplicity == "N")) {
                    var text = Utils.GetFormattedText(StartMultiplicity);
                    Point p4;
                    double angle;
                    GetTextPositionOnCurve(text, 0.0, 23.0, out p4, out angle);
                    dc.PushTransform(new RotateTransform(angle, p4.X, p4.Y));
                    dc.DrawText(
                        text,
                        p4
                    );
                    dc.Pop();
                }

                if (!string.IsNullOrEmpty(EndMultiplicity) && (StartMultiplicity == "N" || EndMultiplicity == "N")) {
                    var text = Utils.GetFormattedText(EndMultiplicity);
                    Point p4;
                    double angle;
                    GetTextPositionOnCurve(text, 1.0, -23.0, out p4, out angle);
                    dc.PushTransform(new RotateTransform(angle, p4.X, p4.Y));
                    dc.DrawText(
                        text,
                        p4
                    );
                    dc.Pop();
                }

                //if (BendingOffset > 0) {
                //    dc.DrawLine(Utils.DefaultPen, GetPointOnCurve(0.5), BendingPoint);
                //}
            }
        }
    }
}
