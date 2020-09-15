using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using DiagramViewer.Models;
using Mono.Cecil;
using Mono.CSharp;

namespace DiagramViewer.ViewModels {
    public class UmlDiagramClass : DiagramNode {
        //
        // No, maybe not nice to have a cyclic dependency, but i didn't feel like throwing an event from here and 
        // be obliged to unsubscribe in time since a UmlDiagram lives longer than an UmlDiagramClass.
        //
        private UmlDiagram umlDiagram;

        public DelegateCommand CloseCommand { get; private set; }

        

        public UmlDiagramClass(UmlClass umlClass, UmlDiagram umlDiagram) {
            Class = umlClass;
            this.umlDiagram = umlDiagram;

            SyncAttributesAndOperationsFromModel();

            attributes.CollectionChanged += (sender, args) => NotifyPropertyChanged(() => HasAttributes);
            properties.CollectionChanged += (sender, args) => NotifyPropertyChanged(() => HasProperties);
            operations.CollectionChanged += (sender, args) => NotifyPropertyChanged(() => HasOperations);

            relations = new ObservableCollection<UmlDiagramRelation>();
            Relations = new ReadOnlyObservableCollection<UmlDiagramRelation>(relations);

            CloseCommand = new DelegateCommand(OnExecuteCloseCommand);
        }

        public ReadOnlyObservableCollection<UmlDiagramRelation> Relations { get; private set; }
        private readonly ObservableCollection<UmlDiagramRelation> relations;

        public void AddRelation(UmlDiagramRelation umlDiagramRelation) {
            AddLink(umlDiagramRelation);
            if (!relations.Contains(umlDiagramRelation)) {
                relations.Add(umlDiagramRelation);
            }
        }

        public void RemoveRelation(UmlDiagramRelation umlDiagramRelation) {
            RemoveLink(umlDiagramRelation);
            if (relations.Contains(umlDiagramRelation)) {
                relations.Remove(umlDiagramRelation);
            }
        }

        private static Brush RedBrush = new SolidColorBrush(Colors.Red);
        private static Brush OrangeBrush = new SolidColorBrush(Colors.Orange);
        private static Brush YellowBrush = new SolidColorBrush(Colors.Yellow);
        private static Brush RoyalBlueBrush = new SolidColorBrush(Colors.RoyalBlue);
        private static Brush CornflowerBlueBrush = new SolidColorBrush(Colors.CornflowerBlue);
        private static Brush LightBlueBrush = new SolidColorBrush(Colors.LightBlue);
        private static Brush ForestGreenBrush = new SolidColorBrush(Colors.ForestGreen);
        private static Brush GreenYellowBrush = new SolidColorBrush(Colors.GreenYellow);
        private static Brush MediumPurpleBrush = new SolidColorBrush(Colors.MediumPurple);
        private static Brush BlanchedAlmondBrush = new SolidColorBrush(Colors.BlanchedAlmond);
        private static Brush BeigeBrush = new SolidColorBrush(Colors.Beige);

        private bool showsPrivateMembers = true;
        public bool ShowsPrivateMembers {
            get { return showsPrivateMembers; }
            set { 
                if(SetProperty(value, ref showsPrivateMembers, () => ShowsPrivateMembers)) {
                    umlDiagram.UpdateMemberVisibility(this);
                }
            }
        }

        private bool showsProtectedMembers = true;
        public bool ShowsProtectedMembers {
            get { return showsProtectedMembers; }
            set {
                if (SetProperty(value, ref showsProtectedMembers, () => ShowsProtectedMembers)) {
                    umlDiagram.UpdateMemberVisibility(this);
                }
            }
        }

        private bool showsInternalMembers = true;
        public bool ShowsInternalMembers {
            get { return showsInternalMembers; }
            set {
                if (SetProperty(value, ref showsInternalMembers, () => ShowsInternalMembers)) {
                    umlDiagram.UpdateMemberVisibility(this);
                }
            }
        }

        private bool showsPublicMembers = true;
        public bool ShowsPublicMembers {
            get { return showsPublicMembers; }
            set {
                if (SetProperty(value, ref showsPublicMembers, () => ShowsPublicMembers)) {
                    umlDiagram.UpdateMemberVisibility(this);
                }
            }
        }

        private bool showsMembers;
        public bool ShowsMembers {
            get { return showsMembers; }
            set { SetProperty(value, ref showsMembers, () => ShowsMembers); }
        }

        public IEnumerable<UmlDiagramClass> SuperClasses {
            get {
                return
                    Relations.OfType<UmlDiagramInheritanceRelation>().Where(r => r.StartClass == this).Select(q => q.EndClass);
            }
        }

        public IEnumerable<UmlDiagramClass> SubClasses {
            get {
                return
                    Relations.OfType<UmlDiagramInheritanceRelation>().Where(r => r.EndClass == this).Select(q => q.StartClass);
            }
        }

        public IEnumerable<UmlDiagramClass> Implementors {
            get {
                return
                    Relations.OfType<UmlDiagramImplementsInterfaceRelation>().Where(r => r.EndClass == this).Select(q => q.StartClass);
            }
        }

        public IEnumerable<UmlDiagramClass> CompositionParentClasses {
            get {
                return
                    Relations.OfType<UmlDiagramCompositionRelation>().Where(r => r.StartClass == this).Select(q => q.EndClass);
            }
        }

        public IEnumerable<UmlDiagramClass> CompositionChildClasses {
            get {
                return
                    Relations.OfType<UmlDiagramCompositionRelation>().Where(r => r.EndClass == this).Select(q => q.StartClass);
            }
        }

        public IEnumerable<UmlDiagramClass> AssociationParentClasses {
            get {
                return
                    Relations.OfType<UmlDiagramAssociationRelation>().Where(r => r.EndClass == this).Select(q => q.StartClass);
            }
        }

        public IEnumerable<UmlDiagramClass> AssociationChildClasses {
            get {
                return
                    Relations.OfType<UmlDiagramAssociationRelation>().Where(r => r.StartClass == this).Select(q => q.EndClass);
            }
        }

        public IEnumerable<UmlDiagramClass> Ancestors {
            get {
                var superClasses = SuperClasses.ToList();
                return superClasses.Union(superClasses.SelectMany(s => s.Ancestors));
            }
        }

        public IEnumerable<UmlDiagramClass> Descendents {
            get {
                var subClasses = SubClasses.ToList();
                return subClasses.Union(subClasses.SelectMany(s => s.Descendents));
            }
        }

        public UmlClass Class { get; private set; }

        public string Name {
            get { return Class.DisplayName; }
        }

        public bool HasAttributes {
            get { return Attributes.Any(m => m.IsVisibleInList); }
        }

        private readonly ObservableCollection<UmlDiagramClassAttribute> attributes = new ObservableCollection<UmlDiagramClassAttribute>();
        public ObservableCollection<UmlDiagramClassAttribute> Attributes {
            get { return attributes; }
        }

        public bool HasProperties {
            get { return Properties.Any(m => m.IsVisibleInList); }
        }

        private readonly ObservableCollection<UmlDiagramClassProperty> properties = new ObservableCollection<UmlDiagramClassProperty>();
        public ObservableCollection<UmlDiagramClassProperty> Properties {
            get { return properties; }
        }

        public bool HasOperations {
            get { return Operations.Any(m => m.IsVisibleInList); }
        }

        private readonly ObservableCollection<UmlDiagramClassOperation> operations = new ObservableCollection<UmlDiagramClassOperation>();
        public ObservableCollection<UmlDiagramClassOperation> Operations {
            get { return operations; }
        }

        public void SyncAttributesAndOperationsFromModel() {
            foreach (var umlAttribute in Class.Attributes.Where(umlAttribute => Attributes.All(a => a.UmlAttribute != umlAttribute))) {
                Attributes.Add(new UmlDiagramClassAttribute(umlAttribute));
            }

            foreach (var umlProperty in Class.Properties.Where(umlProperty => Properties.All(a => a.UmlProperty != umlProperty))) {
                Properties.Add(new UmlDiagramClassProperty(umlProperty));
            }

            foreach (var umlOperation in Class.Operations.Where(umlOperation => Operations.All(c => c.UmlOperation != umlOperation))) {
                Operations.Add(new UmlDiagramClassOperation(umlOperation, Class));
            }
        }

        // IsVisibleInList is currently only set once, in the constructor of this class, to true

        //public void ShowClass() {
        //    IsVisibleInList = true;
        //    IsPositionControlled = false;
        //    ExertsForces = true;
        //    AcceptsForces = true;
        //}

        //public void HideClass() {
        //    IsVisibleInList = false;
        //}

        //public void CollapseClass() {
        //    IsVisibleInList = false;
        //    ExertsForces = false;
        //    AcceptsForces = false;
        //    IsPositionControlled = true;
        //}

        public Brush BackgroundBrush {
            get {
                //
                // TODO: UIElement krijgt hier een gele kleur omdat zijn Assembly xxx blijkt te zijn. We laden PresentationFramework.dll niet expliciet in,
                // dus het lijkt erop dat hij UIElement vanuit een associatie met xxx.dll resolved (wat me sterk lijkt, maar ok).
                //
                if (Tags.Contains("DataModel")) {
                    return MediumPurpleBrush;
                }
                if (Tags.Contains("DataController")) {
                    return OrangeBrush;
                }
                if(Tags.Contains("Controller")) {
                    return RedBrush;
                }
                if (Tags.Contains("ViewModel")) {
                    return YellowBrush;
                }
                if (Tags.Contains("Controller")) {
                    return RedBrush;
                }
                if (Tags.Contains("PresentationState")) {
                    return LightBlueBrush;
                }
                if (Tags.Contains("Presentation")) {
                    return GreenYellowBrush;
                }
                if (Tags.Contains("SceneHandler")) {
                    return MediumPurpleBrush;
                }

                return BeigeBrush;
            }
        }

        public string StereoType {
            get { return Class.StereoType; }
        }

        private void OnExecuteCloseCommand() {
            //umlDiagram.RemoveClassesFromDiagram(new List<UmlClass> { Class });
            umlDiagram.RemoveClassesFromModel(new List<UmlClass> { Class });
        }
    }
}
