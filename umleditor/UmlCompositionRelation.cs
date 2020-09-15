using System.Windows.Media;

namespace UmlEditor {
    public class UmlCompositionRelation : UmlAggregationRelation {

        public UmlCompositionRelation(string preferredAngleString, string startMultiplicity = "1", string endMultiplicity = "1") : base(preferredAngleString, startMultiplicity, endMultiplicity) { }

        protected override Brush GetFillBrush() {
            return Brushes.Black;
        }
    }
}
