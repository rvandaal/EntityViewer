
namespace DiagramViewer.Models {
    public class UmlAggregationRelation : UmlRelation {
        //
        // TODO: SubClass from UmlAssociationRelation. Same in model.
        //
        public UmlAggregationRelation(
            UmlClass startClass, 
            UmlClass endClass, 
            string name = null, 
            string startMultiplicity = "1", 
            string endMultiplicity = "1"
        ) : base(startClass, endClass, name, startMultiplicity, endMultiplicity) { }
    }
}
