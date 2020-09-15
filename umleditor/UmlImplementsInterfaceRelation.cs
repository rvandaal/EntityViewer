using System.Windows.Media;

namespace UmlEditor {
    public class UmlImplementsInterfaceRelation : UmlInheritanceRelation {

        public UmlImplementsInterfaceRelation(string preferredAngleString) : base(preferredAngleString) { }

        protected override Pen GetMainLinePen() {
            Pen pen = new Pen(new SolidColorBrush(Colors.Black), 1);
            pen.DashStyle = new DashStyle(new[] {4.0, 4.0}, 0.0);
            return pen;
        }
    }
}
