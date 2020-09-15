
namespace DiagramViewer.Models {
    public class UmlCompositionRelation : UmlAggregationRelation {
        public UmlCompositionRelation(UmlClass startClass, UmlClass endClass, string name = null, string startMultiplicity = "1", string endMultiplicity = "1") : 
            base(startClass, endClass, name, startMultiplicity, endMultiplicity) { }
    }
}
