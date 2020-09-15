using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace DiagramViewer.Models {
    public class UmlClass : Node {
        private readonly List<UmlRelation> relations;

        public IList<UmlRelation> Relations {
            get { return relations.AsReadOnly(); }
        }

        public void AddRelation(UmlRelation umlRelation) {
            if (!relations.Contains(umlRelation)) {
                AddLink(umlRelation);
                relations.Add(umlRelation);
            }
        }

        public void RemoveRelation(UmlRelation umlRelation) {
            if (relations.Contains(umlRelation)) {
                RemoveLink(umlRelation);
                Relations.Remove(umlRelation);
            }
        }

        public string StereoType { get; set; }

        public string Name { get; set; }
        public string DisplayName { get; private set; }
        public AssemblyDefinition AssemblyDefinition { get; set; }
        public TypeDefinition TypeDefinition { get; set; }
        public Collection<GenericParameter> GenericParameters;

        private readonly List<UmlAttribute> attributes = new List<UmlAttribute>();
        public List<UmlAttribute> Attributes {
            get { return attributes; }
        }

        private readonly List<UmlProperty> properties = new List<UmlProperty>();
        public List<UmlProperty> Properties {
            get { return properties; }
        }

        private readonly List<UmlOperation> operations = new List<UmlOperation>();
        public List<UmlOperation> Operations {
            get { return operations; }
        } 

        public UmlClass(
            TypeDefinition type, 
            AssemblyDefinition assembly
        ) {
            if (type != null) {
                Name = type.Name;
                GenericParameters = type.GenericParameters;
                SetDisplayName();
                AssemblyDefinition = assembly;
                TypeDefinition = type;
                relations = new List<UmlRelation>();
            }
        }

        public UmlClass(string name) {
            Name = name;
            SetDisplayName();
            relations = new List<UmlRelation>();
        }

        private void SetDisplayName() {
            string name = Name;
            int index = name.IndexOf("`");
            if (index > -1) {
                name = name.Substring(0, index);
            }
            if (GenericParameters != null && GenericParameters.Any()) {
                name += "<" + GenericParameters[0].Name;
                foreach (var typeArgument in GenericParameters.Skip(1)) {
                    name += "," + typeArgument.Name;
                }
                name += ">";
            }
            DisplayName = name;
        }

        public IEnumerable<UmlClass> SuperClasses {
            get {
                return
                    Relations.OfType<UmlInheritanceRelation>().Where(r => r.StartClass == this).Select(q => q.EndClass);
            }
        }

        public IEnumerable<UmlClass> SubClasses {
            get {
                return
                    Relations.OfType<UmlInheritanceRelation>().Where(r => r.EndClass == this).Select(q => q.StartClass);
            }
        }

        public IEnumerable<UmlClass> ImplementedInterfaces {
            get {
                return
                    Relations.OfType<UmlImplementsInterfaceRelation>().Where(r => r.StartClass == this).Select(q => q.EndClass);
            }
        }

        public IEnumerable<UmlClass> Implementors {
            get {
                return
                    Relations.OfType<UmlImplementsInterfaceRelation>().Where(r => r.EndClass == this).Select(q => q.StartClass);
            }
        }

        public IEnumerable<UmlClass> CompositionParentClasses {
            get {
                return
                    Relations.OfType<UmlCompositionRelation>().Where(r => r.StartClass == this).Select(q => q.EndClass);
            }
        }

        public IEnumerable<UmlClass> CompositionChildClasses {
            get {
                return
                    Relations.OfType<UmlCompositionRelation>().Where(r => r.EndClass == this).Select(q => q.StartClass);
            }
        }

        public IEnumerable<UmlClass> AssociationParentClasses {
            get {
                return
                    Relations.OfType<UmlAssociationRelation>().Where(r => r.EndClass == this).Select(q => q.StartClass);
            }
        }

        public IEnumerable<UmlClass> AssociationChildClasses {
            get {
                return
                    Relations.OfType<UmlAssociationRelation>().Where(r => r.StartClass == this).Select(q => q.EndClass);
            }
        }

        public IEnumerable<UmlClass> Ancestors {
            get {
                var superClasses = SuperClasses.ToList();
                return superClasses.Union(superClasses.SelectMany(s => s.Ancestors));
            }
        }

        public IEnumerable<UmlClass> Descendents {
            get {
                var subClasses = SubClasses.ToList();
                return subClasses.Union(subClasses.SelectMany(s => s.Descendents));
            }
        }

        public IEnumerable<UmlMethodNode> MethodNodes {
            get {
                return
                    Relations.OfType<UmlMethodLink>().Where(r => r.Class == this).Select(q => q.MethodNode);
            }
        }
    }
}
