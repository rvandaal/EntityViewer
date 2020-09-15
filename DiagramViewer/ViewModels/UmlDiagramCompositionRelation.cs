using System.Windows.Media;
using DiagramViewer.Models;
using DiagramViewer.Utilities;

namespace DiagramViewer.ViewModels {
    public class UmlDiagramCompositionRelation : UmlDiagramAggregationRelation {
        public UmlDiagramCompositionRelation(
            UmlRelation umlRelation, 
            UmlDiagramClass startUmlDiagramClass, 
            UmlDiagramClass endUmlDiagramClass
        ) : base(umlRelation, startUmlDiagramClass, endUmlDiagramClass) {
            PreferredAngles = new double[] {  };
        }

        protected override Brush GetFillBrush() {
            return Utils.DefaultForegroundBrush;
        }
    }
}
