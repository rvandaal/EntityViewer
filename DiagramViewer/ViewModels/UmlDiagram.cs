using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using DiagramViewer.Models;
using Microsoft.Win32;
using Mono.Cecil;

namespace DiagramViewer.ViewModels {

    public enum RelationType {
        None,
        Inheritance,
        Aggregation,
        Composition,
        Association,
        ImplementsInterface,
        Dependence,
        HasMethod
    }

    public class UmlDiagram : Diagram {

        private readonly Random rand = new Random();

        private readonly Dictionary<UmlClass, UmlDiagramClass> modelToDiagramClasses = new Dictionary<UmlClass, UmlDiagramClass>();
        private readonly Dictionary<UmlRelation, UmlDiagramRelation> modelToDiagramRelations = new Dictionary<UmlRelation, UmlDiagramRelation>();

        public UmlModel Model { get; private set; }

        public DelegateCommand CopyDiagramToClipboardCommand { get; private set; }
        public DelegateCommand OpenAssemblyCommand { get; private set; }
        public DelegateCommand LoadAssemblyCommand { get; private set; }

        // Waarom hebben we niet gewoon een Node collection ipv classes, notes, methodnodes, etc etc?
        // Ja ok we willen ze niet alleen layouten, functioneel willen we idd wel onderscheid maken tussen deze objecten.


        #region Classes

        public ReadOnlyObservableCollection<UmlDiagramClass> Classes { get; private set; }
        private readonly ObservableCollection<UmlDiagramClass> classes;

        private void AddClass(UmlDiagramClass umlDiagramClass) {
            if (!classes.Contains(umlDiagramClass)) {
                AddNode(umlDiagramClass);
                classes.Add(umlDiagramClass);
                if(!ClassNames.Contains(umlDiagramClass.Name)) {
                    ClassNames.Add(umlDiagramClass.Name);
                }
            }
        }

        private void RemoveClass(UmlDiagramClass umlDiagramClass) {
            if (classes.Contains(umlDiagramClass)) {
                RemoveNode(umlDiagramClass);
                classes.Remove(umlDiagramClass);
            }
        }

        #endregion

        #region Relations

        public ReadOnlyObservableCollection<UmlDiagramRelation> Relations { get; private set; }
        private readonly ObservableCollection<UmlDiagramRelation> relations;

        private void AddRelation(UmlDiagramRelation umlDiagramRelation) {
            if (!relations.Contains(umlDiagramRelation)) {
                AddLink(umlDiagramRelation);
                relations.Add(umlDiagramRelation);
            }
        }

        private void RemoveRelation(UmlDiagramRelation umlDiagramRelation) {
            if (relations.Contains(umlDiagramRelation)) {
                RemoveLink(umlDiagramRelation);
                relations.Remove(umlDiagramRelation);
                UpdateBendingOffsets(umlDiagramRelation.StartClass, umlDiagramRelation.EndClass);
            }
        }

        #endregion

        #region Notes

        public ReadOnlyObservableCollection<UmlDiagramNote> Notes { get; private set; }
        private readonly ObservableCollection<UmlDiagramNote> notes;

        private void AddNote(UmlDiagramNote umlDiagramNote) {
            if (!notes.Contains(umlDiagramNote)) {
                AddNode(umlDiagramNote);
                notes.Add(umlDiagramNote);
            }
        }

        private void RemoveNote(UmlDiagramNote umlDiagramNote) {
            if (notes.Contains(umlDiagramNote)) {
                RemoveNode(umlDiagramNote);
                notes.Remove(umlDiagramNote);
            }
        }

        #endregion

        #region NoteLinks

        public ReadOnlyObservableCollection<UmlDiagramNoteLink> NoteLinks { get; private set; }
        private readonly ObservableCollection<UmlDiagramNoteLink> noteLinks;

        private void AddNoteLink(UmlDiagramNoteLink umlDiagramNoteLink) {
            if (!noteLinks.Contains(umlDiagramNoteLink)) {
                AddLink(umlDiagramNoteLink);
                noteLinks.Add(umlDiagramNoteLink);
            }
        }

        private void RemoveNoteLink(UmlDiagramNoteLink umlDiagramNoteLink) {
            if (noteLinks.Contains(umlDiagramNoteLink)) {
                RemoveLink(umlDiagramNoteLink);
                noteLinks.Remove(umlDiagramNoteLink);
            }
        }

        #endregion

        #region MethodNodes

        public ReadOnlyObservableCollection<UmlDiagramMethodNode> MethodNodes { get; private set; }
        private readonly ObservableCollection<UmlDiagramMethodNode> methodNodes;

        private void AddMethodNode(UmlDiagramMethodNode umlDiagramMethodNode)
        {
            if (!methodNodes.Contains(umlDiagramMethodNode))
            {
                AddNode(umlDiagramMethodNode);
                methodNodes.Add(umlDiagramMethodNode);
            }
        }

        private void RemoveMethodNode(UmlDiagramMethodNode umlDiagramMethodNode)
        {
            if (methodNodes.Contains(umlDiagramMethodNode))
            {
                RemoveNode(umlDiagramMethodNode);
                methodNodes.Remove(umlDiagramMethodNode);
            }
        }

        #endregion

        #region MethodLinks

        public ReadOnlyObservableCollection<UmlDiagramMethodLink> MethodLinks { get; private set; }
        private readonly ObservableCollection<UmlDiagramMethodLink> methodLinks;

        private void AddMethodLink(UmlDiagramMethodLink umlDiagramMethodLink)
        {
            if (!methodLinks.Contains(umlDiagramMethodLink))
            {
                AddLink(umlDiagramMethodLink);
                methodLinks.Add(umlDiagramMethodLink);
            }
        }

        private void RemoveMethodLink(UmlDiagramMethodLink umlDiagramMethodLink)
        {
            if (methodLinks.Contains(umlDiagramMethodLink))
            {
                RemoveLink(umlDiagramMethodLink);
                methodLinks.Remove(umlDiagramMethodLink);
            }
        }

        #endregion

        private bool showsMembers;
        public bool ShowsMembers {
            get { return showsMembers; }
            set { 
                if(SetProperty(value, ref showsMembers, () => ShowsMembers)) {
                    foreach(var umlDiagramClass in Classes) {
                        umlDiagramClass.ShowsMembers = value;
                    }
                } 
            }
        }

        public UmlDiagram(UmlModel model) {
            Model = model;
            model.Modified += OnModelModified;

            //model.EditsCommitted += OnModelEditsCommitted;
            model.AssemblyLoaded += OnAssemblyLoaded;

            classes = new ObservableCollection<UmlDiagramClass>();
            Classes = new ReadOnlyObservableCollection<UmlDiagramClass>(classes);
            relations = new ObservableCollection<UmlDiagramRelation>();
            Relations = new ReadOnlyObservableCollection<UmlDiagramRelation>(relations);
            notes = new ObservableCollection<UmlDiagramNote>();
            Notes = new ReadOnlyObservableCollection<UmlDiagramNote>(notes);
            noteLinks = new ObservableCollection<UmlDiagramNoteLink>();
            NoteLinks = new ReadOnlyObservableCollection<UmlDiagramNoteLink>(noteLinks);

            methodNodes = new ObservableCollection<UmlDiagramMethodNode>();
            MethodNodes = new ReadOnlyObservableCollection<UmlDiagramMethodNode>(methodNodes);
            methodLinks = new ObservableCollection<UmlDiagramMethodLink>();
            MethodLinks = new ReadOnlyObservableCollection<UmlDiagramMethodLink>(methodLinks);

            CopyDiagramToClipboardCommand = new DelegateCommand(OnExecuteCopyDiagramToClipboardCommand);
            OpenAssemblyCommand = new DelegateCommand(OnExecuteOpenAssemblyCommand);
            LoadAssemblyCommand = new DelegateCommand(OnExecuteLoadAssemblyCommand);
            ClassNames = new ObservableCollection<string>();
            Tags = new ObservableCollection<string> {"DataController", "Controller", "ViewModel", "UI", "DataModel", "PresentationState", "Presentation", "SceneHandler"};
            SyncClassNames();
            filenames = new List<string> { @"C:\Repos\wc3\Output\x3.dll", @"C:\Repos\wc3\Output\x2.exe", @"C:\Repos\wc3\Output\x1.dll", @"C:\Repos\wc3\Output\DataModel.dll" };
            UpdateFilenamesString();
        }

        private void OnAssemblyLoaded(object sender, EventArgs e) {
            SyncClassNames();
        }

        private void OnModelEditsCommitted(object sender, EventArgs e) {
            ReApplyOperations();
        }

        public ObservableCollection<string> ClassNames { get; private set; }
        public ObservableCollection<string> Tags { get; private set; }

        /// <summary>
        /// Adds a class to the Model and the diagram.
        /// </summary>
        public UmlClass CreateModelClass(string name) {
            return Model.CreateClassFromDiagram(name);
        }

        private void OnModelModified(object sender, EventArgs e) {
            //
            // Model has, ViewModel has not
            //
            foreach (var umlClass in Model.Classes) {
                if (GetDiagramClass(umlClass) == null) {
                    AddClassFromModel(umlClass);
                }
            }
            foreach (var umlRelation in Model.Relations) {
                if (GetDiagramRelation(umlRelation) == null) {
                    AddRelationFromModel(umlRelation);
                }
            }
            //
            // ViewModel has, Model has not
            //
            List<UmlDiagramClass> classesToRemove = new List<UmlDiagramClass>();
            foreach(var umlDiagramClass in Classes) {
                if(!Model.Classes.Contains(umlDiagramClass.Class)) {
                    classesToRemove.Add(umlDiagramClass);
                }
            }
            foreach(var umlDiagramClass in classesToRemove) {
                RemoveDiagramClass(umlDiagramClass);
                // At this point, we know that the umlClass is not present in the Model.
                // Hence, at this point we should remove the name from the class list (autocompletebox).
                if (ClassNames.Contains(umlDiagramClass.Name)) {
                    ClassNames.Remove(umlDiagramClass.Name);
                }
            }

            List<UmlDiagramRelation> relationsToRemove = new List<UmlDiagramRelation>();
            foreach (var umlDiagramRelation in Relations) {
                if (!Model.Relations.Contains(umlDiagramRelation.Relation)) {
                    relationsToRemove.Add(umlDiagramRelation);
                }
            }
            foreach (var umlDiagramRelation in relationsToRemove) {
                RemoveDiagramRelation(umlDiagramRelation);
            }
        }

        public UmlRelation CreateModelRelation(
            RelationType relationType,
            UmlClass startClass,
            UmlClass endClass,
            bool oneToN,
            string name = null
        ) {
            UmlRelation umlRelation = null;

            switch (relationType) {
                case RelationType.Inheritance:
                    umlRelation = Model.GetOrCreateInheritanceRelation(true, startClass, endClass);
                    break;
                case RelationType.ImplementsInterface:
                    umlRelation = Model.GetOrCreateImplementsInterfaceRelation(true, startClass, endClass);
                    break;
                case RelationType.Aggregation:
                    umlRelation = Model.GetOrCreateAggregationRelation(true, startClass, endClass, name);
                    umlRelation.StartMultiplicity = oneToN ? "N" : "1";
                    break;
                case RelationType.Composition:
                    umlRelation = Model.GetOrCreateCompositionRelation(true, startClass, endClass, name);
                    umlRelation.StartMultiplicity = oneToN ? "N" : "1";
                    break;
                case RelationType.Association:
                    umlRelation = Model.GetOrCreateAssociationRelation(true, startClass, endClass, name);
                    umlRelation.EndMultiplicity = oneToN ? "N" : "1";
                    break;
                case RelationType.Dependence:
                    umlRelation = Model.GetOrCreateDependenceRelation(true, startClass, endClass, name);
                    umlRelation.EndMultiplicity = oneToN ? "N" : "1";
                    break;
            }

            if (umlRelation != null) {
                //
                // Exception for now: immediately create the relation in the diagram too. This should happen via an event from the model,
                // but for now, we do it directly.
                //
                //AddRelationFromModel(umlRelation);
            }
            return umlRelation;
        }

        public void RemoveModelRelation(
            UmlClass startClass,
            UmlClass endClass,
            RelationType relationType = RelationType.None,
            string name = null
        ) {
            if (relationType == RelationType.None) {
                Model.RemoveRelations(startClass, endClass, name);
            } else {
                switch (relationType) {
                    case RelationType.Inheritance:
                        Model.RemoveRelation<UmlInheritanceRelation>(startClass, endClass, name);
                        break;
                    case RelationType.ImplementsInterface:
                        Model.RemoveRelation<UmlImplementsInterfaceRelation>(startClass, endClass, name);
                        break;
                    case RelationType.Aggregation:
                        Model.RemoveRelation<UmlAggregationRelation>(startClass, endClass, name);
                        break;
                    case RelationType.Composition:
                        Model.RemoveRelation<UmlCompositionRelation>(startClass, endClass, name);
                        break;
                    case RelationType.Association:
                        Model.RemoveRelation<UmlAssociationRelation>(startClass, endClass, name);
                        break;
                    case RelationType.Dependence:
                        Model.RemoveRelation<UmlDependenceRelation>(startClass, endClass, name);
                        break;
                }
            }
        }

        public void CreateModelNote(UmlClass umlClass, string note) {
            UmlNote umlNote;
            UmlNoteLink umlNoteLink;
            Model.CreateNoteFromDiagram(umlClass, note, out umlNote, out umlNoteLink);
            if(umlNote != null && umlNoteLink != null) {
                AddNoteFromModel(umlClass, umlNote, umlNoteLink);
            }
        }

        public void CreateModelMethodNode(UmlClass umlClass, string methodName)
        {
            UmlMethodNode umlMethodNode;
            UmlMethodLink umlMethodLink;
            Model.CreateMethodNodeFromDiagram(umlClass, methodName, out umlMethodNode, out umlMethodLink);
            if (umlMethodNode != null && umlMethodLink != null)
            {
                AddMethodNodeFromModel(umlClass, umlMethodNode, umlMethodLink);
            }
        }

        /// <summary>
        /// Add a class that already exists in the Model to the diagram.
        /// </summary>
        private void AddClassFromModel(UmlClass umlClass) {
            var umlDiagramClass = new UmlDiagramClass(umlClass, this) { ShowsMembers = ShowsMembers};
            CheckForTags(umlDiagramClass);
            Model.FillMethodsFromClass(umlClass);
            umlDiagramClass.SyncAttributesAndOperationsFromModel();
            modelToDiagramClasses[umlClass] = umlDiagramClass;
            umlDiagramClass.Pos = new Point(rand.NextDouble() * 300, rand.NextDouble() * 300);
            AddClass(umlDiagramClass);
            
            //
            // Add any relation that should be visible in the diagram now.
            //
            //
            // Don't just add all umlClass.Relations, only the ones that are connected to an
            // already shown class.
            //
            foreach (var umlDiagramClass2 in Classes) {
                List<UmlRelation> umlRelations =
                    Model.Relations.Where(
                        r =>
                        r.StartClass == umlClass && r.EndClass == umlDiagramClass2.Class ||
                        r.StartClass == umlDiagramClass2.Class && r.EndClass == umlClass
                    ).ToList();
                foreach (var umlRelation in umlRelations) {
                    UmlDiagramRelation umlDiagramRelation = GetDiagramRelation(umlRelation);
                    if (umlDiagramRelation == null) {
                        AddRelationFromModel(umlRelation);
                    }
                }
            }

            NotifyPropertyChanged(() => ClassNames);
        }

        private void CheckForTags(UmlDiagramClass umlDiagramClass) {
            UmlClass umlClass = umlDiagramClass.Class;

            foreach(var tag in Tags) {
                if (umlDiagramClass.Name.ToLower().Contains(tag.ToLower())) {
                    if (tag == "PresentationState" || tag == "Presentation") {
                        umlDiagramClass.AddTag("DataModel");
                    } else {
                        umlDiagramClass.AddTag(tag);
                    }
                    break;
                }

                if (umlClass.AssemblyDefinition != null) {
                    if (umlClass.AssemblyDefinition.FullName.Contains("Controllers")) {
                        umlDiagramClass.AddTag("Controller");
                    }
                    if (umlClass.AssemblyDefinition.FullName.Contains("ViewModels")) {
                        umlDiagramClass.AddTag("ViewModel");
                    }
                    if (umlClass.AssemblyDefinition.FullName.Contains("UI")) {
                        umlDiagramClass.AddTag("UI");
                    }
                    if (umlClass.AssemblyDefinition.FullName.Contains("DataModel")) {
                        umlDiagramClass.AddTag("DataModel");
                    }
                }
            }
        }

        public UmlDiagramRelation AddRelationFromModel(UmlRelation umlRelation) {

            var startUmlDiagramClass = Classes.FirstOrDefault(c => c.Class == umlRelation.StartClass);
            var endUmlDiagramClass = Classes.FirstOrDefault(c => c.Class == umlRelation.EndClass);
            UmlDiagramRelation umlDiagramRelation = null;

            if (umlRelation is UmlDependenceRelation) umlDiagramRelation = new UmlDiagramDependenceRelation(umlRelation, startUmlDiagramClass, endUmlDiagramClass);
            else if (umlRelation is UmlAssociationRelation) umlDiagramRelation = new UmlDiagramAssociationRelation(umlRelation, startUmlDiagramClass, endUmlDiagramClass);
            else if (umlRelation is UmlImplementsInterfaceRelation) umlDiagramRelation = new UmlDiagramImplementsInterfaceRelation(umlRelation, startUmlDiagramClass, endUmlDiagramClass);
            else if (umlRelation is UmlInheritanceRelation) umlDiagramRelation = new UmlDiagramInheritanceRelation(umlRelation, startUmlDiagramClass, endUmlDiagramClass);
            else if (umlRelation is UmlCompositionRelation) umlDiagramRelation = new UmlDiagramCompositionRelation(umlRelation, startUmlDiagramClass, endUmlDiagramClass);
            else if (umlRelation is UmlAggregationRelation) umlDiagramRelation = new UmlDiagramAggregationRelation(umlRelation, startUmlDiagramClass, endUmlDiagramClass);

            if (umlDiagramRelation != null && startUmlDiagramClass != null && endUmlDiagramClass != null) {
                modelToDiagramRelations[umlRelation] = umlDiagramRelation;
                AddRelation(umlDiagramRelation);
                startUmlDiagramClass.AddRelation(umlDiagramRelation);
                endUmlDiagramClass.AddRelation(umlDiagramRelation);
            }

            UpdateBendingOffsets(endUmlDiagramClass, startUmlDiagramClass);

            return umlDiagramRelation;
        }

        public void RemoveDiagramClass(UmlDiagramClass umlDiagramClass) {
            if(Classes.Contains(umlDiagramClass)) {
                RemoveClass(umlDiagramClass);
                modelToDiagramClasses.Remove(umlDiagramClass.Class);

                //List<UmlDiagramRelation> umlDiagramRelations =
                //    Relations.Where(
                //        r =>
                //        r.StartClass == umlDiagramClass || r.EndClass == umlDiagramClass
                //    ).ToList();

                //foreach (var umlDiagramRelation in umlDiagramRelations) {
                //    RemoveDiagramRelation(umlDiagramRelation);
                //}
            }
        }

        public void RemoveDiagramRelation(UmlDiagramRelation umlDiagramRelation) {
            RemoveRelation(umlDiagramRelation);
            modelToDiagramRelations.Remove(umlDiagramRelation.Relation);
        }

        public void AddNoteFromModel(UmlClass umlClass, UmlNote umlNote, UmlNoteLink umlNoteLink) {
            UmlDiagramClass umlDiagramClass = GetDiagramClass(umlClass);
            var umlDiagramNote = new UmlDiagramNote(umlNote);
            var umlDiagramNoteLink = new UmlDiagramNoteLink(umlNoteLink, umlDiagramClass, umlDiagramNote);
            AddNote(umlDiagramNote);
            AddNoteLink(umlDiagramNoteLink);
        }

        public void AddMethodNodeFromModel(UmlClass umlClass, UmlMethodNode umlMethodNode, UmlMethodLink umlMethodLink)
        {
            UmlDiagramClass umlDiagramClass = GetDiagramClass(umlClass);
            var umlDiagramMethodNode = new UmlDiagramMethodNode(umlMethodNode);
            var umlDiagramMethodLink = new UmlDiagramMethodLink(umlMethodLink, umlDiagramClass, umlDiagramMethodNode);
            AddMethodNode(umlDiagramMethodNode);
            AddMethodLink(umlDiagramMethodLink);
        }

        internal void UpdateMemberVisibility(UmlDiagramClass umlDiagramClass) {
            foreach (var umlAttribute in umlDiagramClass.Attributes) {
                umlAttribute.IsVisibleInList =
                    umlAttribute.AccessModifier == AccessModifier.Private && umlDiagramClass.ShowsPrivateMembers ||
                    umlAttribute.AccessModifier == AccessModifier.Protected && umlDiagramClass.ShowsProtectedMembers ||
                    umlAttribute.AccessModifier == AccessModifier.Internal && umlDiagramClass.ShowsInternalMembers ||
                    umlAttribute.AccessModifier == AccessModifier.Public && umlDiagramClass.ShowsPublicMembers;
            }
            foreach (var umlProperty in umlDiagramClass.Properties) {
                umlProperty.IsVisibleInList =
                    (
                        umlProperty.GetterAccessModifier == AccessModifier.Private ||
                        umlProperty.SetterAccessModifier == AccessModifier.Private
                    ) && umlDiagramClass.ShowsPrivateMembers ||
                    (
                        umlProperty.GetterAccessModifier == AccessModifier.Protected ||
                        umlProperty.SetterAccessModifier == AccessModifier.Protected
                    ) && umlDiagramClass.ShowsProtectedMembers ||
                    (
                        umlProperty.GetterAccessModifier == AccessModifier.Internal ||
                        umlProperty.SetterAccessModifier == AccessModifier.Internal
                    ) && umlDiagramClass.ShowsInternalMembers ||
                    (
                        umlProperty.GetterAccessModifier == AccessModifier.Public ||
                        umlProperty.SetterAccessModifier == AccessModifier.Public
                    ) && umlDiagramClass.ShowsPublicMembers;
            }
            foreach (var umlOperation in umlDiagramClass.Operations) {
                umlOperation.IsVisibleInList =
                    umlOperation.AccessModifier == AccessModifier.Private && umlDiagramClass.ShowsPrivateMembers ||
                    umlOperation.AccessModifier == AccessModifier.Protected && umlDiagramClass.ShowsProtectedMembers ||
                    umlOperation.AccessModifier == AccessModifier.Internal && umlDiagramClass.ShowsInternalMembers ||
                    umlOperation.AccessModifier == AccessModifier.Public && umlDiagramClass.ShowsPublicMembers;
            }

            foreach (var umlDiagramAssociationRelation in umlDiagramClass.Relations.OfType<UmlDiagramAssociationRelation>().Where(w => w.StartClass == umlDiagramClass)) {
                //
                // Search for 'Double dispatch': resolving a virtual method happens dynamically, resolving the type of a method
                // parameter happens statically. So, at this point, the type of the method argument of 'GetAccessmodifier' has to
                // be resolved. Which means we have to explicitly cast MemberReference.
                //
                bool isVisible = umlDiagramAssociationRelation.IsVisible;

                if (umlDiagramAssociationRelation.MemberReference is FieldReference) {
                    var accessModifier =
                        UmlModel.GetAccessmodifier(((FieldReference)umlDiagramAssociationRelation.MemberReference).Resolve());
                    umlDiagramAssociationRelation.IsVisible =
                        accessModifier == AccessModifier.Private && umlDiagramClass.ShowsPrivateMembers ||
                        accessModifier == AccessModifier.Protected && umlDiagramClass.ShowsProtectedMembers ||
                        accessModifier == AccessModifier.Internal && umlDiagramClass.ShowsInternalMembers ||
                        accessModifier == AccessModifier.Public && umlDiagramClass.ShowsPublicMembers;
                } else if (umlDiagramAssociationRelation.MemberReference is MethodReference) {
                    var accessModifier =
                        UmlModel.GetAccessmodifier(((MethodReference)umlDiagramAssociationRelation.MemberReference).Resolve());
                    umlDiagramAssociationRelation.IsVisible =
                        accessModifier == AccessModifier.Private && umlDiagramClass.ShowsPrivateMembers ||
                        accessModifier == AccessModifier.Protected && umlDiagramClass.ShowsProtectedMembers ||
                        accessModifier == AccessModifier.Internal && umlDiagramClass.ShowsInternalMembers ||
                        accessModifier == AccessModifier.Public && umlDiagramClass.ShowsPublicMembers;
                } else if (umlDiagramAssociationRelation.MemberReference is PropertyReference) {
                    var getterAccessModifier =
                        UmlModel.GetAccessmodifier(((PropertyReference)umlDiagramAssociationRelation.MemberReference).Resolve().GetMethod);
                    var setterAccessModifier =
                        UmlModel.GetAccessmodifier(((PropertyReference)umlDiagramAssociationRelation.MemberReference).Resolve().SetMethod);
                    umlDiagramAssociationRelation.IsVisible =
                        (
                            getterAccessModifier == AccessModifier.Private ||
                            setterAccessModifier == AccessModifier.Private
                        ) && umlDiagramClass.ShowsPrivateMembers ||
                        (
                            getterAccessModifier == AccessModifier.Protected ||
                            setterAccessModifier == AccessModifier.Protected
                        ) && umlDiagramClass.ShowsProtectedMembers ||
                        (
                            getterAccessModifier == AccessModifier.Internal ||
                            setterAccessModifier == AccessModifier.Internal
                        ) && umlDiagramClass.ShowsInternalMembers ||
                        (
                            getterAccessModifier == AccessModifier.Public ||
                            setterAccessModifier == AccessModifier.Public
                        ) && umlDiagramClass.ShowsPublicMembers;
                }

                if(umlDiagramAssociationRelation.IsVisible != isVisible) {
                    UpdateBendingOffsets(umlDiagramAssociationRelation.EndClass, umlDiagramClass);
                }
            }
        }

        // TODO: call this method whenever one of the relations is changing its visibility
        private void UpdateBendingOffsets(UmlDiagramClass endUmlDiagramClass, UmlDiagramClass startUmlDiagramClass) {
            var tmprelations =
                Relations.Where(v => v.IsVisible).Where(
                    r => r.StartClass == startUmlDiagramClass && r.EndClass == endUmlDiagramClass ||
                         r.StartClass == endUmlDiagramClass && r.EndClass == startUmlDiagramClass).ToList();

            int numberOfRelationsBetweenStartAndEndClass = tmprelations.Count;
            const double range = 300.0;
            double deltaRange = range/(numberOfRelationsBetweenStartAndEndClass + 1);

            if (numberOfRelationsBetweenStartAndEndClass == 1) {
                tmprelations[0].BendingOffset = 0;
            }
            else {
                for (int i = 0; i < tmprelations.Count; i++) {
                    var currentRelation = tmprelations[i];
                    currentRelation.BendingOffset = (i + 1)*deltaRange - range/2;
                    if (currentRelation.StartClass == endUmlDiagramClass) {
                        //
                        // If the start and end class are reversed, the bendingoffset has to be reversed too, otherwise it will coincide with a previous relation curve.
                        //
                        currentRelation.BendingOffset = -currentRelation.BendingOffset;
                    }
                }
            }
        }

        public UmlDiagramClassAttribute AddAttributeFromModel(UmlClass umlClass, UmlAttribute umlAttribute) {
            var umlDiagramClass = modelToDiagramClasses[umlClass];
            var umlDiagramClassAttribute = new UmlDiagramClassAttribute(umlAttribute);
            umlDiagramClass.Attributes.Add(umlDiagramClassAttribute);
            return umlDiagramClassAttribute;
        }

        public void RemoveAttributeFromModel(UmlClass umlClass, UmlAttribute attributeToRemove) {
            var umlDiagramClass = modelToDiagramClasses[umlClass];
            var attributesToRemove = umlDiagramClass.Attributes.Where(u => u.UmlAttribute == attributeToRemove).ToList();
            foreach (var attribute in attributesToRemove) {
                umlDiagramClass.Attributes.Remove(attribute);
            }
        }

        public UmlDiagramClassOperation AddOperationFromModel(UmlClass umlClass, UmlOperation umlOperation) {
            var umlDiagramClass = modelToDiagramClasses[umlClass];
            var umlDiagramClassOperation = new UmlDiagramClassOperation(umlOperation, umlClass);
            umlDiagramClass.Operations.Add(umlDiagramClassOperation);
            return umlDiagramClassOperation;
        }

        public void RemoveOperationFromModel(UmlClass umlClass, UmlOperation operationToRemove) {
            var umlDiagramClass = modelToDiagramClasses[umlClass];
            var operationsToRemove = umlDiagramClass.Operations.Where(u => u.UmlOperation == operationToRemove).ToList();
            foreach (var operation in operationsToRemove) {
                umlDiagramClass.Operations.Remove(operation);
            }
        }

        public UmlAttribute CreateModelAttribute(UmlClass umlClass, string name, string type) {
            // TODO: check if attribute is already in the model
            var umlAttribute = new UmlAttribute(name, type);
            umlClass.Attributes.Add(umlAttribute);
            //
            // Exception for now: immediately create the attribute in the diagram too. This should happen via an event from the model,
            // but for now, we do it directly.
            //
            AddAttributeFromModel(umlClass, umlAttribute);
            return umlAttribute;
        }

        public void RemoveModelAttribute(UmlClass umlClass, string name, string type = null) {
            IEnumerable<UmlAttribute> attributesToRemove = umlClass.Attributes.Where(o => o.Name == name && (type == null || o.Type == type)).ToList();
            foreach (var attribute in attributesToRemove) {
                umlClass.Attributes.Remove(attribute);

                //
                // Exception for now: immediately remove the attribute in the diagram too. This should happen via an event from the model,
                // but for now, we do it directly.
                //
                RemoveAttributeFromModel(umlClass, attribute);
            }
        }

        public UmlOperation CreateModelOperation(UmlClass umlClass, string name, string type) {
            // TODO: check if operation is already in the model
            var umlOperation = new UmlOperation(name, type);
            umlClass.Operations.Add(umlOperation);
            //
            // Exception for now: immediately create the operation in the diagram too. This should happen via an event from the model,
            // but for now, we do it directly.
            //
            AddOperationFromModel(umlClass, umlOperation);
            return umlOperation;
        }

        public void RemoveModelOperation(UmlClass umlClass, string name, string type = null) {
            IEnumerable<UmlOperation> operationsToRemove = umlClass.Operations.Where(o => o.Name == name && (type == null || o.Type == type)).ToList();
            foreach (var operation in operationsToRemove) {
                umlClass.Operations.Remove(operation);

                //
                // Exception for now: immediately remove the operation in the diagram too. This should happen via an event from the model,
                // but for now, we do it directly.
                //
                RemoveOperationFromModel(umlClass, operation);
            }
        }

        private void OnExecuteCopyDiagramToClipboardCommand() {
            Clipboard.Clear();
            StringBuilder stb = new StringBuilder();
            foreach(string operation in operations) {
                stb.AppendLine(operation);
            }
            Clipboard.SetText(stb.ToString());
        }

        public string FilenamesString { get; private set; }
        private List<string> filenames;

        private void OnExecuteOpenAssemblyCommand() {
            filenames = OpenFileDialog();
            UpdateFilenamesString();
        }

        private void UpdateFilenamesString() {
            string filenameString = "";
            bool first = true;
            if (filenames != null && filenames.Any()) {
                foreach (var filename in filenames) {
                    if (first) {
                        filenameString += Path.GetFileName(filename);
                    } else {
                        filenameString += ", " + Path.GetFileName(filename);
                    }
                    first = false;
                }
                FilenamesString = filenameString;
            }
        }

        private void OnExecuteLoadAssemblyCommand() {
            if (filenames != null && filenames.Any()) {
                foreach (var filename in filenames) {
                    Model.LoadAssembly(filename);
                }
            }
        }

        private static List<string> OpenFileDialog() {
            OpenFileDialog openAsm = new OpenFileDialog {Multiselect = true, Filter = "Assembly | *.exe;*.dll"};
            if (openAsm.ShowDialog() == true) {
                if (openAsm.FileNames != null && openAsm.FileNames.Any()) {
                    return openAsm.FileNames.ToList();
                }
            }
            return null;
        }

        #region Input

        private readonly List<string> operations = new List<string>(); 

        private string inputText;
        public string InputText {
            get { return inputText; }
            set { SetPropertyClass(value, ref inputText, () => InputText); }
        }

        public void CommitInputText() {
            operations.Add(InputText);
            Classes.ToList().ForEach(c => c.CanAnimate = false);
            UmlDiagramInputParser.ProcessInput(InputText, this, false);
            Classes.ToList().ForEach(c => c.CanAnimate = true);
            CommitEdits();
            InputText = "";
        }

        public void ClearModel() {
            Model.ClearModel();
            ClearDiagram();
        }

        public override void ClearDiagram() {
            base.ClearDiagram();
            classes.Clear();
            relations.Clear();
            operations.Clear();
            modelToDiagramClasses.Clear();
            modelToDiagramRelations.Clear();
            SyncClassNames();
        }

        public void SyncClassNames() {
            ClassNames.Clear();
            ClassNames.Add("Show");
            ClassNames.Add("ShowRelation");
            ClassNames.Add("ShowMembers");
            ClassNames.Add("HideMembers");
            ClassNames.Add("ClearModel");
            ClassNames.Add("ClearDiagram");
            ClassNames.Add("ShowAll");
            ClassNames.Add("Hide");
            ClassNames.Add("Pause");
            ClassNames.Add("Resume");
            ClassNames.Add("has");
            ClassNames.Add("hasN");
            ClassNames.Add("hasmethod");
            ClassNames.Add("hasm");
            ClassNames.Add("dependson");
            ClassNames.Add("implements");
            ClassNames.Add("is");
            ClassNames.Add("owns");
            ClassNames.Add("ownsN");
            ClassNames.Add("contains");
            ClassNames.Add("containsN");
            ClassNames.Add("StartAttracting");
            ClassNames.Add("StopAttracting");
            ClassNames.Add("Find");

            foreach(var umlClass in Model.Classes) {
                ClassNames.Add(umlClass.DisplayName);
            }
        }

        public void StartAttracting() {
            UmlDiagramSimulator.StartAttracting();
        }

        public void StopAttracting() {
            UmlDiagramSimulator.StopAttracting();
        }

        public void RemoveClassesFromModel(List<UmlClass> classesToRemove) {
            foreach (var umlClass in classesToRemove) {
                //UmlDiagramClass umlDiagramClass = GetDiagramClass(umlClass);
                //if (umlDiagramClass != null) {
                //    RemoveDiagramClass(umlDiagramClass);
                //}
                if(umlClass != null) {
                    Model.RemoveClass(umlClass, true);
                }
            }
        }

        public void RemoveClassesFromDiagram(List<UmlClass> classes) {
            foreach (var umlClass in classes) {
                UmlDiagramClass umlDiagramClass = GetDiagramClass(umlClass);
                if(umlDiagramClass != null) {
                    RemoveDiagramClass(umlDiagramClass);
                }
            }
        }

        public void ShowAll() {
            Show(Model.Classes.ToList());
        }

        public void Show(List<UmlClass> classes) {
            foreach (var umlClass in classes) {
                UmlDiagramClass umlDiagramClass = GetDiagramClass(umlClass);
                if(umlDiagramClass == null) {
                    AddClassFromModel(umlClass);
                }
            }
        }

        private UmlDiagramClass GetDiagramClass(UmlClass umlClass) {
            return modelToDiagramClasses.ContainsKey(umlClass) ? modelToDiagramClasses[umlClass] : null;
        }

        private UmlDiagramRelation GetDiagramRelation(UmlRelation umlRelation) {
            return modelToDiagramRelations.ContainsKey(umlRelation) ? modelToDiagramRelations[umlRelation] : null;
        }

        public void ShowRelation(List<UmlClass> startClasses, List<UmlClass> endClasses) {
            IEnumerable<UmlClass> classesToBeShown = new List<UmlClass>();
            foreach(var startClass in startClasses) {
                foreach(var endClass in endClasses) {
                    classesToBeShown = classesToBeShown.Union(Model.FindShortestPath(startClass, endClass));
                }
            }
            Show(classesToBeShown.ToList());
        }

        private void ReApplyOperations() {
            Classes.ToList().ForEach(c => c.CanAnimate = false);
            ClearDiagram();
            //foreach(var operation in operations.Where(o => !o.ToLower().StartsWith("hide")).Union(operations.Where(p => p.ToLower().StartsWith("hide")))) {
            //    UmlDiagramInputParser.ProcessInput(operation, this, false);    
            //}
            foreach (var operation in operations) {
                UmlDiagramInputParser.ProcessInput(operation, this, false);
            }
            Classes.ToList().ForEach(c => c.CanAnimate = true);
        }

        public void CommitEdits() {
            Model.CommitEdits();
        }


        #endregion
    }
}
