using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using SDILReader;

namespace DiagramViewer.Models {
    public class UmlModel : Model {

        public static AssemblyDefinition LoadedAssembly { get; set; }

        private static string[] ValueTypes = new[] {"void", "string", "int", "double", "float", "bool", "boolean", "long", "point", "point3d", "vector", "vector3d", "matrix", "matrix3d",
                                                    "int?", "double?", "float?", "bool?", "boolean?", "long?", "point?", "point3d?", "vector?", "vector3d?",
                                                    "observablecollection<string>"};

        //private readonly Dictionary<Type, UmlClass> generatedClasses = new Dictionary<Type, UmlClass>();
        private readonly Dictionary<string, UmlClass> generatedClasses = new Dictionary<string, UmlClass>();
        private readonly List<UmlClass> noAssociationsRetrievedClasses = new List<UmlClass>();

        private bool isDirty;

        #region Classes

        public ReadOnlyCollection<UmlClass> Classes { get; private set; }
        private readonly List<UmlClass> classes;

        public void AddClass(UmlClass umlClass, bool notifyObservers) {
            if (!classes.Contains(umlClass)) {
                AddNode(umlClass);
                classes.Add(umlClass);
                if (notifyObservers) {
                    OnModified();
                }
            }
        }

        public void RemoveClass(UmlClass umlClass, bool notifyObservers) {
            if (classes.Contains(umlClass)) {
                RemoveNode(umlClass);
                classes.Remove(umlClass);
                List<UmlRelation> umlRelations =
                    Relations.Where(
                        r =>
                        r.StartClass == umlClass || r.EndClass == umlClass
                    ).ToList();

                foreach (var umlRelation in umlRelations) {
                    RemoveRelation(umlRelation, notifyObservers);
                }
                if (notifyObservers) {
                    OnModified();
                }
            }
        }

        #endregion

        #region Relations

        public ReadOnlyCollection<UmlRelation> Relations { get; private set; }
        private readonly List<UmlRelation> relations;

        public void AddRelation(UmlRelation umlRelation, bool notifyObservers) {
            if (!relations.Contains(umlRelation)) {
                AddLink(umlRelation);
                relations.Add(umlRelation);
                if(notifyObservers) {
                    OnModified();
                }
            }
        }

        public void RemoveRelation(UmlRelation umlRelation, bool notifyObservers) {
            if (relations.Contains(umlRelation)) {
                RemoveLink(umlRelation);
                relations.Remove(umlRelation);
                if(notifyObservers) {
                    OnModified();
                }
            }
        }

        #endregion

        #region Notes

        public ReadOnlyCollection<UmlNote> Notes { get; private set; }
        private readonly List<UmlNote> notes;

        private void AddNote(UmlNote umlNote) {
            if (!notes.Contains(umlNote)) {
                AddNode(umlNote);
                notes.Add(umlNote);
            }
        }

        private void RemoveNote(UmlNote umlNote) {
            if (notes.Contains(umlNote)) {
                RemoveNode(umlNote);
                notes.Remove(umlNote);
            }
        }

        #endregion


        #region NoteLinks

        public ReadOnlyCollection<UmlNoteLink> NoteLinks { get; private set; }
        private readonly List<UmlNoteLink> noteLinks;

        private void AddNoteLink(UmlNoteLink umlNoteLink)
        {
            if (!noteLinks.Contains(umlNoteLink))
            {
                AddLink(umlNoteLink);
                noteLinks.Add(umlNoteLink);
            }
        }

        private void RemoveNoteLink(UmlNoteLink umlNoteLink)
        {
            if (noteLinks.Contains(umlNoteLink))
            {
                RemoveLink(umlNoteLink);
                noteLinks.Remove(umlNoteLink);
            }
        }

        #endregion

        #region MethodNodes

        public ReadOnlyCollection<UmlMethodNode> MethodNodes { get; private set; }
        private readonly List<UmlMethodNode> methodNodes;

        private void AddMethodNode(UmlMethodNode umlMethodNode)
        {
            if (!methodNodes.Contains(umlMethodNode))
            {
                AddNode(umlMethodNode);
                methodNodes.Add(umlMethodNode);
            }
        }

        private void RemoveMethodNode(UmlMethodNode umlMethodNode)
        {
            if (methodNodes.Contains(umlMethodNode))
            {
                RemoveNode(umlMethodNode);
                methodNodes.Remove(umlMethodNode);
            }
        }

        #endregion

        #region MethodLinks

        public ReadOnlyCollection<UmlMethodLink> MethodLinks { get; private set; }
        private readonly List<UmlMethodLink> methodLinks;

        private void AddMethodLink(UmlMethodLink umlMethodLink)
        {
            if (!methodLinks.Contains(umlMethodLink))
            {
                AddLink(umlMethodLink);
                methodLinks.Add(umlMethodLink);
            }
        }

        private void RemoveMethodLink(UmlMethodLink umlMethodLink)
        {
            if (methodLinks.Contains(umlMethodLink))
            {
                RemoveLink(umlMethodLink);
                methodLinks.Remove(umlMethodLink);
            }
        }

        #endregion

        public event EventHandler Modified;
        public event EventHandler EditsCommitted;
        public event EventHandler AssemblyLoaded;

        public UmlModel() {
            Globals.LoadOpCodes();
            classes = new List<UmlClass>();
            Classes = classes.AsReadOnly();
            relations = new List<UmlRelation>();
            Relations = relations.AsReadOnly();
            notes = new List<UmlNote>();
            Notes = notes.AsReadOnly();
            noteLinks = new List<UmlNoteLink>();
            NoteLinks = noteLinks.AsReadOnly();

            methodNodes = new List<UmlMethodNode>();
            MethodNodes = methodNodes.AsReadOnly();
            methodLinks = new List<UmlMethodLink>();
            MethodLinks = methodLinks.AsReadOnly();
        }

        public override void ClearModel() {
            base.ClearModel();
            classes.Clear();
            relations.Clear();
            notes.Clear();
            noteLinks.Clear();
            methodNodes.Clear();
            methodLinks.Clear();
        }

        private void CopyInheritanceRelation(UmlClass umlClass, TypeReference baseTypeReference) {
            if (baseTypeReference != null) {
                TypeDefinition baseTypeDefinition = baseTypeReference.Resolve();
                var baseClass = baseTypeDefinition != null && baseTypeDefinition.GetType() != typeof (object)
                                    ? AddOrGetClassFromSolution(baseTypeDefinition)
                                    : null;
                if (baseClass != null) {
                    GetOrCreateInheritanceRelation(false, umlClass, baseClass);
                }
            }
        }

        private void CopyImplementsInterfaceRelations(UmlClass umlClass, IEnumerable<TypeReference> interfaceTypeReferences) {
            foreach (var interfaceType in interfaceTypeReferences) {
                CopyImplementsInterfaceRelation(umlClass, interfaceType);
            }
        }
        private void CopyImplementsInterfaceRelation(UmlClass umlClass, TypeReference interfaceTypeReference) {
            var baseClassesAndInterfaces = umlClass.SuperClasses.Concat(umlClass.ImplementedInterfaces);
            if (interfaceTypeReference != null) {
                if (
                    baseClassesAndInterfaces.Any(b => 
                        b.TypeDefinition.HasInterfaces && 
                        b.TypeDefinition.Interfaces.Contains(interfaceTypeReference)
                    )
                ) {
                    return;
                }
                TypeDefinition interfaceTypeDefinition = interfaceTypeReference.Resolve();
                var interfaceClass = interfaceTypeDefinition != null && interfaceTypeDefinition.GetType() != typeof(object)
                                         ? AddOrGetClassFromSolution(interfaceTypeDefinition)
                                         : null;
                if (interfaceClass != null) {
                    GetOrCreateImplementsInterfaceRelation(false, umlClass, interfaceClass);
                }
            }
        }

        private void CopyFields(UmlClass umlClass) {
            var type = umlClass.TypeDefinition;
            if(type == null) {
                return;
            }
            foreach (var field in type.Fields) {
                try {
                    var memberType = field.FieldType;

                    string name;
                    bool isCollection;
                    UmlClass associatedClass;

                    ProcessType(field, ref memberType, out name, out isCollection, out associatedClass);
                    if (associatedClass != null) {
                        GetOrCreateAssociationRelation(false, umlClass, associatedClass, name, "1", isCollection ? "N" : "1", field);
                    }
                    umlClass.Attributes.Add(new UmlAttribute(field.Name, memberType.Name, GetAccessmodifier(field)));
                } catch (Exception) {
                    continue;
                }
            }
            noAssociationsRetrievedClasses.Remove(umlClass);
        }

        private void CopyProperties(UmlClass umlClass) {
            
            var type = umlClass.TypeDefinition;
            if(type == null) {
                // TODO: this happens for example when creating some classes 'a', 'b', 'c', then ClearDiagram and then loading assemblies.
                // c has no type, so: crash
                return;
            }
            foreach (var property in type.Properties) {
                try {
                    var memberType = property.PropertyType;

                    string name;
                    bool isCollection;
                    UmlClass associatedClass;

                    ProcessType(property, ref memberType, out name, out isCollection, out associatedClass);
                    if (associatedClass != null) {
                        GetOrCreateAssociationRelation(false, umlClass, associatedClass, name, "1", isCollection ? "N" : "1", property);
                    }
                    var getter = property.GetMethod;
                    var setter = property.SetMethod;
                    umlClass.Properties.Add(
                        new UmlProperty(
                            property.Name,
                            memberType.Name,
                            getter != null ? GetAccessmodifier(getter) : AccessModifier.None,
                            setter != null ? GetAccessmodifier(setter) : AccessModifier.None
                        )
                    );
                } catch (Exception) {
                    continue;
                }
            }
            noAssociationsRetrievedClasses.Remove(umlClass);
        }

        // If you want to preserve the generic arguments that can go with a method:
        // https://groups.google.com/forum/#!topic/mono-cecil/yRqhqaaYZnA

        public void FillMethodsFromClass(UmlClass umlClass) {
            var type = umlClass.TypeDefinition;

            // NOTE:
            // Fout: we zien nu Operations en Method Associations als verschillende dingen; we moeten nadenken of we de uml conventies van attributes en operations nog
            // wel willen aanhouden. Wat ik interessant vind, is welke fields, properties, methods een class heeft. Misschien moeten we van het hele UML concept afstappen.
            // Ik laat nu nl ook al properties in de view zien, niet volgens UML conventies dus. 
            // Maar toch is UML wel heel puur: attributes staan voor state en operations staan voor behavior. 
            // We maken het dus zo: een 'UmlClass' bevat alles wat nodig is om allebei af te leiden; de attributes en operations kunnen we dan binnen deze class herleiden.
            //

            if (type != null) {
                foreach (var method in type.Methods) {
                    if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")) continue;
                    try {
                        string name;
                        bool isCollection;
                        UmlClass associatedClass;
                        var memberType = method.ReturnType;

                        //ProcessType(method, ref memberType, out name, out isCollection, out associatedClass);
                        //if(associatedClass != null) {
                        //GetOrCreateAssociationRelation(umlClass, associatedClass, name + "()", "1", isCollection ? "N" : "1", method);
                        //} else {
                        umlClass.Operations.Add(new UmlOperation(method.Name, memberType.Name, GetAccessmodifier(method)) { Method = method });
                        //}
                    } catch {
                        continue;
                    }
                }
            }
        }

        public static AccessModifier GetAccessmodifier(FieldDefinition fieldDefinition) {
            if (fieldDefinition == null) {
                return AccessModifier.None;
            }

            if (fieldDefinition.IsPrivate)
                return AccessModifier.Private;
            if (fieldDefinition.IsFamilyAndAssembly)
                return AccessModifier.ProtectedInternal;
            if (fieldDefinition.IsFamily)
                return AccessModifier.Protected;
            if (fieldDefinition.IsAssembly)
                return AccessModifier.Internal;
            if (fieldDefinition.IsPublic)
                return AccessModifier.Public;
            throw new ArgumentException("Did not find access modifier", "fieldDefinition");
        }

        public static AccessModifier GetAccessmodifier(MethodDefinition methodDefinition) {
            if(methodDefinition == null) {
                return AccessModifier.None;
            }
            if (methodDefinition.IsPrivate)
                return AccessModifier.Private;
            if (methodDefinition.IsFamilyAndAssembly)
                return AccessModifier.ProtectedInternal;
            if (methodDefinition.IsFamily)
                return AccessModifier.Protected;
            if (methodDefinition.IsAssembly)
                return AccessModifier.Internal;
            if (methodDefinition.IsPublic)
                return AccessModifier.Public;
            throw new ArgumentException("Did not find access modifier", "methodDefinition");
        }

        private void ProcessType(MemberReference memberReference, ref TypeReference typeReference, out string name, out bool isCollection, out UmlClass associatedClass) {
            associatedClass = null;
            isCollection = false;
            name = null;
            if (!ValueTypes.Contains(typeReference.Name.ToLower())) {
                //
                // Yes, we want the associated class in our diagram
                //
                if (typeReference.Name.StartsWith("IList") || typeReference.Name.StartsWith("List`") || typeReference.Name.StartsWith("IEnumerable") || typeReference.Name.StartsWith("ObservableCollection")) {
                    var genericParameters = ((GenericInstanceType) typeReference).GenericArguments;
                    if (genericParameters.Any()) {
                        typeReference = genericParameters[0];
                        isCollection = true;
                    }
                }
                name = memberReference.Name;
                if (name.EndsWith("BackingField")) {
                    name = name.Substring(1, name.IndexOf(">", StringComparison.InvariantCulture) - 1);
                }
                associatedClass = AddOrGetClassFromSolution(typeReference.Resolve());
            }
        }

        public List<UmlClass> FindShortestPath(UmlClass class1, UmlClass class2) {
            // Dijkstra's algorithm: https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm

            UmlClass currentClass;
            Dictionary<UmlClass, int> distances = new Dictionary<UmlClass, int>();
            
            classes.ForEach(c => distances[c] = int.MaxValue);
            distances[class1] = 0;
            List<UmlClass> unvisitedClasses = Classes.ToList();
            Dictionary<UmlClass, UmlClass> parents = new Dictionary<UmlClass, UmlClass>();

            while(unvisitedClasses.Any() && unvisitedClasses.Contains(class2)) {
                currentClass = unvisitedClasses.Aggregate((m, c) => distances[m] < distances[c] ? m : c);
                int currentDistance = distances[currentClass];
                List<UmlClass> neighbours =
                    currentClass.SuperClasses.Union(currentClass.SubClasses).Union(currentClass.CompositionParentClasses)
                        .Union(currentClass.CompositionChildClasses).Union(currentClass.AssociationParentClasses).Union(
                            currentClass.AssociationChildClasses).ToList();
                //List<UmlClass> neighbours =
                //    currentClass.SubClasses
                //        .Union(currentClass.CompositionChildClasses).Union(
                //            currentClass.AssociationChildClasses).ToList();
                foreach(var neighbour in neighbours) {
                    const int distanceToNeighbour = 1;
                    if (neighbour.Name.Contains("Event") || neighbour.Name.Contains("DelegateCommand") || neighbour.Name.Contains("DisplayClass") || neighbour.Name.Contains("`")) {
                        continue;
                    }
                    if(distances[neighbour] > currentDistance + distanceToNeighbour) {
                        distances[neighbour] = currentDistance + distanceToNeighbour;
                        parents[neighbour] = currentClass;
                    }
                }
                unvisitedClasses.Remove(currentClass);
            }
            
            // class2 is visited
            // trace back route
            List<UmlClass> route = new List<UmlClass>();
            currentClass = class2;
            route.Add(class2);
            while (distances[currentClass] > 0) {
                currentClass = parents[currentClass];
                route.Insert(0, currentClass);
            }

            return route;
        }

        public UmlClass CreateClassFromDiagram(string name) {
            if (Classes.All(c => c.Name != name)) {
                var umlClass = new UmlClass(name);
                AddClass(umlClass, true);
                return umlClass;
            }
            return null;
        }

        public UmlClass GetOrCreateClassFromSolution(TypeDefinition typeDefinition) {
            // comparing typeDefinition to typeDefinition gives strange results in which the name is the same ('a') but the fullname can be 'am+ l' ?!
            var umlClass = Classes.FirstOrDefault(c => c.Name == typeDefinition.Name); 
            if (umlClass == null) {
                umlClass = new UmlClass(typeDefinition, LoadedAssembly);
                AddClass(umlClass, false);
            }
            return umlClass;
        }

        public void CreateNoteFromDiagram(UmlClass umlClass, string note, out UmlNote umlNote, out UmlNoteLink umlNoteLink) {
            umlNote = new UmlNote(note);
            umlNoteLink = new UmlNoteLink(umlClass, umlNote);
            AddNote(umlNote);
            AddNoteLink(umlNoteLink);
        }

        public void CreateMethodNodeFromDiagram(UmlClass umlClass, string methodName, out UmlMethodNode umlMethodNode, out UmlMethodLink umlMethodLink)
        {
            umlMethodNode = new UmlMethodNode(methodName);
            umlMethodLink = new UmlMethodLink(umlClass, umlMethodNode);
            AddMethodNode(umlMethodNode);
            AddMethodLink(umlMethodLink);
        }

        public UmlRelation GetOrCreateAssociationRelation(
            bool fromDiagram,
            UmlClass startClass, 
            UmlClass endClass, 
            string name = null, 
            string startMultiplicity = "1", 
            string endMultiplicity = "1",
            MemberReference memberReference = null
        ) {
            var rel = GetRelation<UmlAssociationRelation>(startClass, endClass, name, startMultiplicity,
                                                           endMultiplicity);
            if (rel != null) return rel;

            var umlRelation = new UmlAssociationRelation(startClass, endClass, name, startMultiplicity, endMultiplicity, memberReference);
            AddRelation(umlRelation, fromDiagram);
            return umlRelation;
        }

        public UmlRelation GetOrCreateDependenceRelation(
            bool fromDiagram,
            UmlClass startClass,
            UmlClass endClass,
            string name = null,
            string startMultiplicity = "1",
            string endMultiplicity = "1",
            MemberInfo memberInfo = null
        ) {
            var rel = GetRelation<UmlDependenceRelation>(startClass, endClass, name, startMultiplicity,
                                                           endMultiplicity);
            if (rel != null) return rel;

            var umlRelation = new UmlDependenceRelation(startClass, endClass, name, startMultiplicity, endMultiplicity, memberInfo);
            AddRelation(umlRelation, fromDiagram);
            return umlRelation;
        }

        public UmlRelation GetOrCreateAggregationRelation(
            bool fromDiagram,
            UmlClass startClass, 
            UmlClass endClass, 
            string name = null,
            string startMultiplicity = "1", 
            string endMultiplicity = "1"
        ) {
            var rel = GetRelation<UmlAggregationRelation>(startClass, endClass, name, startMultiplicity,
                                               endMultiplicity);
            if (rel != null) return rel;

            var umlRelation = new UmlAggregationRelation(startClass, endClass, name, startMultiplicity, endMultiplicity);
            AddRelation(umlRelation, fromDiagram);
            return umlRelation;
        }

        /// <summary>
        /// StartClass owns EndClass.
        /// </summary>
        public UmlRelation GetOrCreateCompositionRelation(
            bool fromDiagram,
            UmlClass startClass, 
            UmlClass endClass, 
            string name = null,
            string startMultiplicity = "1", 
            string endMultiplicity = "1"
        ) {
            var rel = GetRelation<UmlCompositionRelation>(startClass, endClass, name, startMultiplicity,
                                               endMultiplicity);
            if (rel != null) return rel;

            rel = GetRelation<UmlAssociationRelation>(endClass, startClass, name, startMultiplicity,
                                               endMultiplicity);
            if(rel != null) {
                //
                // Replace association relation by (stronger) composition relation.
                //
                RemoveRelation(rel, false);
            }

            var umlRelation = new UmlCompositionRelation(startClass, endClass, name, startMultiplicity, endMultiplicity);
            AddRelation(umlRelation, fromDiagram);
            return umlRelation;
        }

        public UmlRelation GetOrCreateInheritanceRelation(bool fromDiagram, UmlClass startClass, UmlClass endClass) {
            UmlRelation rel = GetRelation<UmlInheritanceRelation>(startClass, endClass);
            if (rel == null) {
                rel = new UmlInheritanceRelation(startClass, endClass);
                AddRelation(rel, fromDiagram);
            }
            return rel;
        }

        public UmlRelation GetOrCreateImplementsInterfaceRelation(bool fromDiagram, UmlClass startClass, UmlClass endClass) {
            UmlRelation rel = GetRelation<UmlImplementsInterfaceRelation>(startClass, endClass);
            if (rel == null) {
                rel = new UmlImplementsInterfaceRelation(startClass, endClass);
                AddRelation(rel, fromDiagram);
            }
            return rel;
        }

        public void RemoveRelations(UmlClass startClass, UmlClass endClass, string name = null) {
            var relationsToRemove = GetRelations(startClass, endClass, name);
            foreach (var relation in relationsToRemove) {
                RemoveRelation(relation, true);
            }
        }

        public void RemoveRelation<T>(UmlClass startClass, UmlClass endClass, string name = null) {
            var relationToRemove = GetRelation<T>(startClass, endClass, name);
            RemoveRelation(relationToRemove, true);
        }

        private UmlRelation GetRelation<T>(UmlClass startClass, UmlClass endClass, string name = null, string startMultiplicity = "1", string endMultiplicity = "1") {
            var rel =
                Relations.FirstOrDefault(
                    r => r is T && r.StartClass == startClass && r.EndClass == endClass && (name == null || r.Label.ToLower() == name.ToLower())
                );
            if (rel != null) {
                if (rel.StartMultiplicity != startMultiplicity || rel.EndMultiplicity != endMultiplicity) {
                    throw new ArgumentException("Relation already exists but StartMultiplicity or EndMultiplicity is different");
                }
                return rel;
            }
            return null;
        }

        private IEnumerable<UmlRelation> GetRelations(UmlClass startClass, UmlClass endClass, string name = null) {
            return
                Relations.Where(
                    r => r.StartClass == startClass && r.EndClass == endClass && (name == null || r.Label.ToLower() == name.ToLower())
                );
        }

        private void OnModified() {
            isDirty = true;
            if(Modified != null) {
                Modified(this, null);
            }
        }

        public void CommitEdits() {
            if(EditsCommitted != null){ //} && isDirty) {
                EditsCommitted(this, null);
            }
            isDirty = false;
        }

        internal void LoadAssembly(string filename) {

            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(filename));

            var parameters = new ReaderParameters {AssemblyResolver = resolver};

            LoadedAssembly = AssemblyDefinition.ReadAssembly(filename, parameters);
            IEnumerator enumerator = LoadedAssembly.MainModule.Types.GetEnumerator();
            while (enumerator.MoveNext()) {
                TypeDefinition td = (TypeDefinition) enumerator.Current;
                AddOrGetClassFromSolution(td);
            }

            while (noAssociationsRetrievedClasses.Any()) {
                var list = noAssociationsRetrievedClasses.ToList();
                foreach (var umlClass in list) {
                    CopyProperties(umlClass);
                    CopyFields(umlClass);
                }
            }

            if (AssemblyLoaded != null) {
                AssemblyLoaded(this, null);
            }
        }

        private UmlClass AddOrGetClassFromSolution(TypeDefinition typeDefinition) {
            var baseType = typeDefinition.BaseType;
            if (!generatedClasses.ContainsKey(typeDefinition.Name)) {
                var umlClass = GetOrCreateClassFromSolution(typeDefinition);
                Debug.Assert(umlClass != null);
                generatedClasses[typeDefinition.Name] = umlClass;
                noAssociationsRetrievedClasses.Add(umlClass);

                CopyInheritanceRelation(umlClass, baseType);
                CopyImplementsInterfaceRelations(umlClass, typeDefinition.Interfaces);

                if (typeDefinition.IsValueType) {
                    umlClass.StereoType = "<<struct>>";
                } else if (typeDefinition.IsInterface) {
                    umlClass.StereoType = "<<interface>>";
                }
                return umlClass;
            }
            return generatedClasses[typeDefinition.Name];
        }
    }
}
