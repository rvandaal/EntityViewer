using System.Windows.Media;
using DiagramViewer.Models;

namespace DiagramViewer.ViewModels {
    public class UmlDiagramImplementsInterfaceRelation : UmlDiagramInheritanceRelation {

        public UmlDiagramImplementsInterfaceRelation(
            UmlRelation umlRelation,
            UmlDiagramClass startUmlDiagramClass,
            UmlDiagramClass endUmlDiagramClass
        ) : base(umlRelation, startUmlDiagramClass, endUmlDiagramClass) {
        }

        protected override Pen GetMainLinePen() {
            Pen pen = new Pen(new SolidColorBrush(Colors.Black), 1);
            pen.DashStyle = new DashStyle(new[] { 4.0, 4.0 }, 0.0);
            return pen;
        }
    }
}
