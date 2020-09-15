using System.Collections.Generic;

namespace UmlViewer.Models {
    public class UmlModel {

        public List<UmlClass> UmlClasses { get; private set; }
        public List<UmlRelation> UmlRelations { get; private set; }

        public UmlModel() {
            UmlClasses = new List<UmlClass>();
            UmlRelations = new List<UmlRelation>();
            CreateTestData();
        }

        private void CreateTestData() {
            UmlClasses.Add(new UmlClass { Name = "Class1" });
            UmlClasses.Add(new UmlClass { Name = "Class2" });
            UmlClasses.Add(new UmlClass { Name = "Class3" });
            UmlRelations.Add(new UmlInheritanceRelation{ Class1 = UmlClasses[0], Class2 = UmlClasses[1]});
            UmlRelations.Add(new UmlInheritanceRelation { Class1 = UmlClasses[1], Class2 = UmlClasses[2] });

        }
    }
}
