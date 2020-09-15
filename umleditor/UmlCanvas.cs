using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GraphFramework;
using System.Windows.Media.Media3D;

namespace UmlEditor {
    /// <summary>
    /// Custom Canvas that is reponsible for generating containers and drawing all objects.
    /// </summary>
    /// https://social.msdn.microsoft.com/Forums/vstudio/en-US/32b95d6b-9c6f-4f45-8f3a-989823d8922e/animation-stuttering-with-compositiontargetrender-with-or-without-d3dimage?forum=wpf
    public class UmlCanvas : Canvas {

        #region Private fields

        //private readonly SolidColorBrush nodeBrush = new SolidColorBrush(Colors.Red);
        //private readonly Pen defaultPen = new Pen(new SolidColorBrush(Colors.Black), 1);

        private bool simulateOneTimeStep;

        Vector offsetVector = new Vector(0,0);

        private string[] valueTypes = new[] {"void", "string", "int", "double", "float", "bool", "boolean", "long", "point", "point3d", "vector", "vector3d", "matrix", "matrix3d",
                                             "int?", "double?", "float?", "bool?", "boolean?", "long?", "point?", "point3d?", "vector?", "vector3d?",
                                                "observablecollection<string>"};

        //private readonly Pen redPen = new Pen(new SolidColorBrush(Colors.Red), 1) { DashStyle = new DashStyle(new[] { 4.0, 4.0 }, 0.0) };
        //private readonly Pen greenPen = new Pen(new SolidColorBrush(Colors.Green), 1) { DashStyle = new DashStyle(new[] { 4.0, 4.0 }, 0.0) };
        //private readonly Pen bluePen = new Pen(new SolidColorBrush(Colors.Blue), 1) { DashStyle = new DashStyle(new[] { 4.0, 4.0 }, 0.0) };
        //private readonly Pen brownPen = new Pen(new SolidColorBrush(Colors.Brown), 1) { DashStyle = new DashStyle(new[] { 4.0, 4.0 }, 0.0) };
        //private readonly Pen whitePen = new Pen(new SolidColorBrush(Colors.White), 1) { DashStyle = new DashStyle(new[] { 4.0, 4.0 }, 0.0) };

        private readonly Pen redPen = new Pen(new SolidColorBrush(Colors.Red), 1);
        private readonly Pen greenPen = new Pen(new SolidColorBrush(Colors.Green), 1);
        private readonly Pen bluePen = new Pen(new SolidColorBrush(Colors.Blue), 1);
        private readonly Pen brownPen = new Pen(new SolidColorBrush(Colors.Brown), 1);
        private readonly Pen whitePen = new Pen(new SolidColorBrush(Colors.White), 1);
        private readonly Pen blackPen = new Pen(new SolidColorBrush(Colors.Black), 1) { DashStyle = new DashStyle(new[] { 4.0, 4.0 }, 0.0) };

        private readonly double halfsqrt2 = Math.Sqrt(2)/2;
        private readonly Matrix rotation45Matrix;
        private readonly Matrix rotationMin45Matrix;

        private bool started;
        private DateTime previousTime;
        private double fpsTime;
        private int fpsCount;
        private int lastFpsCount;

        private bool isMouseLeftButtonDown;
        private bool isMouseRightButtonDown;

        private Point startDraggingPoint;

        private readonly Dictionary<UmlClass, UmlClassControl> nodeToUmlClassControl = new Dictionary<UmlClass, UmlClassControl>();
        private readonly Dictionary<UmlClassControl, UmlClass> umlClassControlToNode = new Dictionary<UmlClassControl, UmlClass>();

        #endregion

        #region Constructor

        public UmlCanvas() {
            rotation45Matrix = new Matrix(halfsqrt2, -halfsqrt2, halfsqrt2, halfsqrt2, 0, 0);
            rotationMin45Matrix = new Matrix(halfsqrt2, halfsqrt2, -halfsqrt2, halfsqrt2, 0, 0);

            SizeChanged += OnSizeChanged;
            //MouseRightButtonUp += OnMouseRightButtonUp;
            MouseLeave += OnMouseLeave;
            MouseWheel += OnMouseWheel;

            AddHandler(
                UmlClassControl.MouseLeftButtonUpOnBodyEvent,
                new RoutedEventHandler(OnMouseLeftButtonUpOnNodeBody)
            );

            AddHandler(
                UmlClassControl.MouseLeftButtonUpOnInheritanceConnectorEvent, 
                new RoutedEventHandler(OnMouseLeftButtonUpOnInheritanceConnector)
            );

            AddHandler(
                UmlClassControl.MouseLeftButtonUpOnCompositionConnectorEvent,
                new RoutedEventHandler(OnMouseLeftButtonUpOnCompositionConnector)
            );

            AddHandler(
                UmlClassControl.MouseLeftButtonUpOnAssociationConnectorEvent,
                new RoutedEventHandler(OnMouseLeftButtonUpOnAssociationConnector)
            );

            StepCommand = new DelegateCommand(OnExecuteStepCommand);
            CopyDiagramToClipboardCommand = new DelegateCommand(OnExecuteCopyDiagramToClipboardCommand);

            NodeNames = new ObservableCollection<string>();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, EventArgs e) {
            Simulator.Graph = Diagram;
            OnSizeChanged(null, null);
            //CompositionTarget.Rendering += OnCompositionTargetRendering;
            CompositionTargetEx.FrameUpdating += OnCompositionTargetRendering;
        }

        #endregion

        #region Public properties

        #region Fps

        public int Fps {
            get { return (int)GetValue(FpsProperty); }
            set { SetValue(FpsProperty, value); }
        }

        public static readonly DependencyProperty FpsProperty =
            DependencyProperty.Register(
                "Fps",
                typeof(int),
                typeof(UmlCanvas),
                new FrameworkPropertyMetadata(0)
            );

        #endregion

        #region KineticEnergy

        public double KineticEnergy {
            get { return (double)GetValue(KineticEnergyProperty); }
            set { SetValue(KineticEnergyProperty, value); }
        }

        public static readonly DependencyProperty KineticEnergyProperty =
            DependencyProperty.Register(
                "KineticEnergy",
                typeof(double),
                typeof(UmlCanvas),
                new FrameworkPropertyMetadata(0.0)
            );

        #endregion

        #region IsSimulating

        public bool IsSimulating {
            get { return (bool)GetValue(IsSimulatingProperty); }
            set { SetValue(IsSimulatingProperty, value); }
        }

        public static readonly DependencyProperty IsSimulatingProperty =
            DependencyProperty.Register(
                "IsSimulating",
                typeof(bool),
                typeof(UmlCanvas),
                new FrameworkPropertyMetadata(true, OnIsSimulatingChanged)
            );

        private static void OnIsSimulatingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var canvas = (UmlCanvas)d;
            canvas.Diagram.IsSimulating = (bool)e.NewValue;
        }

        #endregion

        #region IsInEditMode

        public bool IsInEditMode {
            get { return (bool)GetValue(IsInEditModeProperty); }
            set { SetValue(IsInEditModeProperty, value); }
        }

        public static readonly DependencyProperty IsInEditModeProperty =
            DependencyProperty.Register(
                "IsInEditMode",
                typeof(bool),
                typeof(UmlCanvas),
                new FrameworkPropertyMetadata(false, OnIsInEditModeChanged)
            );

        private static void OnIsInEditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var canvas = (UmlCanvas)d;
            canvas.Diagram.IsInEditMode = (bool)e.NewValue;
        }

        #endregion

        #region ShowsNodeOverlay

        public static bool GetShowsNodeOverlay(UIElement element) {
            return (bool)element.GetValue(ShowsNodeOverlayProperty);
        }

        public static void SetShowsNodeOverlay(UIElement element, bool value) {
            element.SetValue(ShowsNodeOverlayProperty, value);
        }

        public static readonly DependencyProperty ShowsNodeOverlayProperty =
            DependencyProperty.RegisterAttached(
                "ShowsNodeOverlay", 
                typeof (bool), 
                typeof (UmlCanvas), 
                new FrameworkPropertyMetadata(
                    false, 
                    FrameworkPropertyMetadataOptions.Inherits
                )
           );

        #endregion

        #region Diagram

        public UmlDiagram Diagram {
            get { return (UmlDiagram)GetValue(DiagramProperty); }
            set { SetValue(DiagramProperty, value); }
        }

        public static readonly DependencyProperty DiagramProperty =
            DependencyProperty.Register(
                "Diagram",
                typeof(UmlDiagram),
                typeof(UmlCanvas),
                new FrameworkPropertyMetadata(null, OnDiagramChanged)
            );

        private static void OnDiagramChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var canvas = (UmlCanvas)d;
            var diagram = (Diagram)e.NewValue;
            canvas.SubscribeForNodesChanged(diagram);
            canvas.SubscribeForSimulationStateChanged(diagram);
            canvas.SubscribeForEditModeChanged(diagram);
            canvas.SubscribeForNodeOverlayChanged(diagram);
            canvas.InitNodes();
        }

        #endregion

        #region Simulator

        public GraphSimulator Simulator {
            get { return (GraphSimulator)GetValue(SimulatorProperty); }
            set { SetValue(SimulatorProperty, value); }
        }

        public static readonly DependencyProperty SimulatorProperty =
            DependencyProperty.Register(
                "Simulator",
                typeof(GraphSimulator),
                typeof(UmlCanvas),
                new FrameworkPropertyMetadata(null)
            );

        #endregion

        #region DrawsForces

        public bool DrawsForces {
            get { return (bool)GetValue(DrawsForcesProperty); }
            set { SetValue(DrawsForcesProperty, value); }
        }

        public static readonly DependencyProperty DrawsForcesProperty =
            DependencyProperty.Register(
                "DrawsForces",
                typeof(bool),
                typeof(UmlCanvas),
                new FrameworkPropertyMetadata(false)
            );

        #endregion

        #region DrawsLinkLabels

        public bool DrawsLinkLabels {
            get { return (bool)GetValue(DrawsLinkLabelsProperty); }
            set { SetValue(DrawsLinkLabelsProperty, value); }
        }

        public static readonly DependencyProperty DrawsLinkLabelsProperty =
            DependencyProperty.Register(
                "DrawsLinkLabels",
                typeof(bool),
                typeof(UmlCanvas),
                new FrameworkPropertyMetadata(false)
            );

        #endregion

        #region DrawsWhenStable

        public bool DrawsWhenStable {
            get { return (bool)GetValue(DrawsWhenStableProperty); }
            set { SetValue(DrawsWhenStableProperty, value); }
        }

        public static readonly DependencyProperty DrawsWhenStableProperty =
            DependencyProperty.Register(
                "DrawsWhenStable",
                typeof(bool),
                typeof(UmlCanvas),
                new FrameworkPropertyMetadata(false)
            );

        #endregion

        #region TimeSpeedFactor

        public double TimeSpeedFactor {
            get { return (double)GetValue(TimeSpeedFactorProperty); }
            set { SetValue(TimeSpeedFactorProperty, value); }
        }

        public static readonly DependencyProperty TimeSpeedFactorProperty =
            DependencyProperty.Register(
                "TimeSpeedFactor",
                typeof(double),
                typeof(UmlCanvas),
                new FrameworkPropertyMetadata(10.0)
            );

        #endregion

        #region InheritanceRelationPreferredAngles

        public string InheritanceRelationPreferredAngles {
            get { return (string)GetValue(InheritanceRelationPreferredAnglesProperty); }
            set { SetValue(InheritanceRelationPreferredAnglesProperty, value); }
        }

        public static readonly DependencyProperty InheritanceRelationPreferredAnglesProperty =
            DependencyProperty.Register("InheritanceRelationPreferredAngles", typeof(string), typeof(UmlCanvas),
                new FrameworkPropertyMetadata("90", OnInheritanceRelationPreferredAnglesChanged)
            );

        private static void OnInheritanceRelationPreferredAnglesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var umlCanvas = (UmlCanvas) d;
            umlCanvas.OnInheritanceRelationPreferredAnglesChanged();
        }

        private void OnInheritanceRelationPreferredAnglesChanged() {
            foreach(var link in Diagram.Links.OfType<UmlInheritanceRelation>()) {
                link.SetPreferredAngles(InheritanceRelationPreferredAngles);
            }
        }

        #endregion

        #region AggregationRelationPreferredAngles

        public string AggregationRelationPreferredAngles {
            get { return (string)GetValue(AggregationRelationPreferredAnglesProperty); }
            set { SetValue(AggregationRelationPreferredAnglesProperty, value); }
        }

        public static readonly DependencyProperty AggregationRelationPreferredAnglesProperty =
            DependencyProperty.Register("AggregationRelationPreferredAngles", typeof(string), typeof(UmlCanvas),
                new FrameworkPropertyMetadata("135", OnAggregationRelationPreferredAnglesChanged)
            );

        private static void OnAggregationRelationPreferredAnglesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var umlCanvas = (UmlCanvas)d;
            umlCanvas.OnAggregationRelationPreferredAnglesChanged();
        }

        private void OnAggregationRelationPreferredAnglesChanged() {
            foreach (var link in Diagram.Links.OfType<UmlAggregationRelation>()) {
                link.SetPreferredAngles(AggregationRelationPreferredAngles);
            }
        }

        #endregion

        #region CompositionRelationPreferredAngles

        public string CompositionRelationPreferredAngles {
            get { return (string)GetValue(CompositionRelationPreferredAnglesProperty); }
            set { SetValue(CompositionRelationPreferredAnglesProperty, value); }
        }

        public static readonly DependencyProperty CompositionRelationPreferredAnglesProperty =
            DependencyProperty.Register("CompositionRelationPreferredAngles", typeof(string), typeof(UmlCanvas),
                new FrameworkPropertyMetadata("135", OnCompositionRelationPreferredAnglesChanged)
            );

        private static void OnCompositionRelationPreferredAnglesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var umlCanvas = (UmlCanvas)d;
            umlCanvas.OnCompositionRelationPreferredAnglesChanged();
        }

        private void OnCompositionRelationPreferredAnglesChanged() {
            foreach (var link in Diagram.Links.OfType<UmlCompositionRelation>()) {
                link.SetPreferredAngles(CompositionRelationPreferredAngles);
            }
        }

        #endregion

        #region AssociationRelationPreferredAngles

        public string AssociationRelationPreferredAngles {
            get { return (string)GetValue(AssociationRelationPreferredAnglesProperty); }
            set { SetValue(AssociationRelationPreferredAnglesProperty, value); }
        }

        public static readonly DependencyProperty AssociationRelationPreferredAnglesProperty =
            DependencyProperty.Register("AssociationRelationPreferredAngles", typeof(string), typeof(UmlCanvas),
                new FrameworkPropertyMetadata("0", OnAssociationRelationPreferredAnglesChanged)
            );

        private static void OnAssociationRelationPreferredAnglesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var umlCanvas = (UmlCanvas)d;
            umlCanvas.OnAssociationRelationPreferredAnglesChanged();
        }

        private void OnAssociationRelationPreferredAnglesChanged() {
            foreach (var link in Diagram.Links.OfType<UmlAssociationRelation>()) {
                link.SetPreferredAngles(AssociationRelationPreferredAngles);
            }
        }

        #endregion

        public ObservableCollection<string> NodeNames { get; private set; }

        #region CreatorText

        private StringBuilder totalStringBuilder = new StringBuilder();

        public string CreatorText {
            get { return (string)GetValue(CreatorTextProperty); }
            set { SetValue(CreatorTextProperty, value); }
        }

        public static readonly DependencyProperty CreatorTextProperty =
            DependencyProperty.Register(
                "CreatorText",
                typeof(string),
                typeof(UmlCanvas),
                new FrameworkPropertyMetadata(null, OnCreatorTextChanged)
            );

        private static void OnCreatorTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var canvas = (UmlCanvas)d;
            canvas.OnCreatorTextChanged((string)e.NewValue);
        }

        private enum CreatorAction {
            None,
            DeletingClass,
            Renaming,
            RenamingTo,
            ResettingClass
        }

        private CreatorAction currentCreatorAction;

        private void OnCreatorTextChanged(string newCreatorText) {
            UmlClass umlClassBeforeOperator = null;
            Link link = null;
            if (string.IsNullOrWhiteSpace(newCreatorText)) return;

            if(newCreatorText.EndsWith("\r\n")) {
                currentCreatorAction = CreatorAction.None;
                if(totalStringBuilder.Length > 0) {
                    totalStringBuilder.Append(" ");
                }
                totalStringBuilder.Append(newCreatorText.Substring(0, newCreatorText.Length - 2));

                //Commit creator text
                string[] tokens = newCreatorText.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                bool endsWithDot = false;
                bool endsWithColon = false;
                bool endsWithN = false;
                bool not = false;
                foreach(string token in tokens) {
                    string token2 = token.Trim();
                    if(token.EndsWith(".")) {
                        endsWithDot = true;
                        token2 = token2.Substring(0, token.Length - 1);
                    }
                    if (token.EndsWith(":")) {
                        endsWithColon = true;
                        token2 = token2.Substring(0, token.Length - 1);
                    }
                    if (token.EndsWith("N")) {
                        endsWithN = true;
                        token2 = token2.Substring(0, token.Length - 1);
                    }
                    switch (token2.ToLower()) {
                        case "reset":
                            totalStringBuilder.Clear();
                            Diagram.Reset();
                            CreatorText = null;
                            return;
                        case "resetclass":
                            currentCreatorAction = CreatorAction.ResettingClass;
                            break;
                        case "not":
                            not = true;
                            break;
                        case "delete":
                            currentCreatorAction = CreatorAction.DeletingClass;
                            break;
                        case "rename":
                            currentCreatorAction = CreatorAction.Renaming;
                            break;
                        case "to":
                            if (currentCreatorAction == CreatorAction.Renaming) {
                                currentCreatorAction = CreatorAction.RenamingTo;
                            }
                            break;
                        case "is":
                            link = new UmlInheritanceRelation(InheritanceRelationPreferredAngles);
                            break;
                        case "implements":
                            link = new UmlImplementsInterfaceRelation(InheritanceRelationPreferredAngles);
                            break;
                        case "dependson":
                        case "uses":
                            link = new UmlDependsOnRelation(AssociationRelationPreferredAngles);
                            break;
                        case "owns":
                        case "creates":
                            link = new UmlCompositionRelation(CompositionRelationPreferredAngles, endsWithN ? "N" : "1");
                            break;
                        case "contains":
                            link = new UmlAggregationRelation(AggregationRelationPreferredAngles, endsWithN ? "N" : "1");
                            break;
                        case "has":
                            link = new UmlAssociationRelation(AssociationRelationPreferredAngles);
                            break;
                        default:
                            if (link == null) {
                                if(currentCreatorAction == CreatorAction.DeletingClass) {
                                    umlClassBeforeOperator = GetOrCreateUmlClass(token2);
                                    Diagram.DeleteNode(umlClassBeforeOperator);
                                    umlClassBeforeOperator = null;
                                    currentCreatorAction = CreatorAction.None;
                                    break;
                                }
                                if(currentCreatorAction == CreatorAction.RenamingTo && umlClassBeforeOperator != null) {
                                    umlClassBeforeOperator.Name = token2;
                                    currentCreatorAction = CreatorAction.None;
                                    break;
                                }
                                if(currentCreatorAction == CreatorAction.ResettingClass) {
                                    umlClassBeforeOperator = GetOrCreateUmlClass(token2);
                                    umlClassBeforeOperator.BackgroundBrush = null;
                                    umlClassBeforeOperator = null;
                                    currentCreatorAction = CreatorAction.None;
                                    break;
                                }
                                if (currentCreatorAction == CreatorAction.None && endsWithColon) {
                                    // Current token is a shortcut for 'has <action>:subject' -> '<action>: subject'
                                    link = new UmlAssociationRelation(AssociationRelationPreferredAngles);
                                    break;
                                }
                                umlClassBeforeOperator = GetOrCreateUmlClass(token2);
                            } else {
                                string name = null;
                                int indexOfColon = token2.IndexOf(":"); // ':' indicates value type. Optional are access modifier and type
                                if(indexOfColon > -1) {
                                    // Value type
                                    AccessModifier am = AccessModifier.None;
                                    int indexOfName = 0;
                                    if(token2.StartsWith("-")) { am = AccessModifier.Private; indexOfName = 1; }
                                    if(token2.StartsWith("#")) { am = AccessModifier.Protected; indexOfName = 1; }
                                    if(token2.StartsWith("$#")) { am = AccessModifier.ProtectedInternal; indexOfName = 2; }
                                    if(token2.StartsWith("$")) { am = AccessModifier.Internal; indexOfName = 1; }
                                    if(token2.StartsWith("+")) { am = AccessModifier.Public; indexOfName = 1; }

                                    name = token2.Substring(indexOfName, indexOfColon - indexOfName);
                                    if(indexOfColon < token2.Length - 1) {
                                        // colon is not the last character, so there is a type specified
                                        string type = token2.Substring(indexOfColon + 1);
                                        var lowerType = type.ToLower();
                                        if(!valueTypes.Contains(lowerType) && !name.EndsWith("()")) {
                                            // Reference type and no operation
                                            token2 = type;
                                            link.Label = name;
                                        } else {
                                            // value type
                                            bool isOperation = name.EndsWith("()");
                                            if (isOperation) {
                                                name = name.Substring(0, name.Length - 2);
                                                umlClassBeforeOperator.Operations.Add(new UmlOperation(name, type, am));
                                            } else {
                                                umlClassBeforeOperator.Attributes.Add(new UmlAttribute(name, type, am));
                                            }
                                            link = null;
                                            // TODO: not
                                            break;
                                        }
                                    } else {
                                        // value type
                                        bool isOperation = name.EndsWith("()");
                                        if (isOperation) {
                                            name = name.Substring(0, name.Length - 2);
                                            umlClassBeforeOperator.Operations.Add(new UmlOperation(name, null, am));
                                        } else {
                                            umlClassBeforeOperator.Attributes.Add(new UmlAttribute(name, null, am));
                                        }
                                        link = null;
                                        // TODO: not
                                        break;
                                    }
                                }
                                var umlClassAfterOperator = GetOrCreateUmlClass(token2);
                                if (umlClassBeforeOperator != null && umlClassAfterOperator != null) {
                                    if (not) {
                                        List<Link> linksToDelete = new List<Link>();
                                        foreach(var linkToDelete in umlClassBeforeOperator.Links) {
                                            if(
                                                linkToDelete.GetType() == link.GetType() && 
                                                (
                                                    (linkToDelete is UmlInheritanceRelation || linkToDelete is UmlAssociationRelation) &&
                                                    linkToDelete.StartNode == umlClassBeforeOperator && linkToDelete.EndNode == umlClassAfterOperator ||
                                                    linkToDelete.StartNode == umlClassAfterOperator && linkToDelete.EndNode == umlClassBeforeOperator
                                                )
                                            ) {
                                                linksToDelete.Add(linkToDelete);
                                            }
                                        }
                                        foreach(var linkToDelete in linksToDelete) {
                                            Diagram.DeleteLink(linkToDelete);
                                        }
                                    } else {
                                        if (link is UmlInheritanceRelation || link is UmlAssociationRelation) {
                                            // StartNode <link> EndNode
                                            link.StartNode = umlClassBeforeOperator;
                                            link.EndNode = umlClassAfterOperator;
                                        }
                                        else {
                                            link.EndNode = umlClassBeforeOperator;
                                            link.StartNode = umlClassAfterOperator;
                                        }
                                        umlClassBeforeOperator.Links.Add(link);
                                        umlClassAfterOperator.Links.Add(link);
                                        Diagram.Links.Add(link);
                                    }
                                    link = null;
                                    not = false;
                                    if(endsWithDot) {
                                        umlClassBeforeOperator = umlClassAfterOperator;
                                    }
                                }
                            }
                            break;
                    }
                    if(endsWithColon && link != null) {
                        link.Label = token2.Replace("_", " ");
                    }
                    endsWithColon = false;
                    endsWithDot = false;
                    endsWithN = false;
                }
                CreatorText = "";
            }
        }

        private UmlClass GetOrCreateUmlClass(string name) {
            UmlClass returnUmlClass = null;
            if (!string.IsNullOrEmpty(name)) {
                var umlClass = Diagram.Nodes.Cast<UmlClass>().FirstOrDefault(c => c.Name == name);
                returnUmlClass = umlClass ?? Diagram.CreateClass(name);
            }
            return returnUmlClass;
        }

        #endregion

        public DelegateCommand StepCommand { get; private set; }

        public DelegateCommand CopyDiagramToClipboardCommand { get; private set; }

        #region FilterText

        public string FilterText {
            get { return (string)GetValue(FilterTextProperty); }
            set { SetValue(FilterTextProperty, value); }
        }

        public static readonly DependencyProperty FilterTextProperty =
            DependencyProperty.Register(
                "FilterText",
                typeof(string),
                typeof(UmlCanvas),
                new FrameworkPropertyMetadata(null, OnFilterTextChanged)
            );

        private static void OnFilterTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var canvas = (UmlCanvas)d;
            canvas.OnFilterTextChanged((string)e.OldValue, (string)e.NewValue);
        }

        private void OnFilterTextChanged(string oldFilterText, string newFilterText) {            
            //
            // TODO: dit trucje gaat niet meer op want een query woord kan nu beginnen met een -
            bool isFilterTextEmpty = string.IsNullOrEmpty(newFilterText);
            foreach (var node in Diagram.Nodes) node.IsVisible = isFilterTextEmpty;
            foreach (var link in Diagram.Links) link.IsVisible = true;

            bool shouldFilter = !string.IsNullOrEmpty(newFilterText);

            if(!shouldFilter && string.IsNullOrEmpty(oldFilterText)) {
                return;
            }

            if (shouldFilter) {
                string[] words = newFilterText != null ? newFilterText.Split(' ') : new string[0];

                //TODO: intialize included to true if all words start with a '-'
                bool allWordsStartWithMin = words.All(w => w.StartsWith("-"));

                // bv: -auto ext con => all nodes that contain ext or con, but not auto

                foreach (var node in Diagram.Nodes) {
                    var name = ((UmlClass) node).Name.ToLower();
                    bool included = false;
                    foreach (var word in words) {
                        string lword = word.ToLower();
                        string sword = lword.Substring(1);
                        //
                        // If a node name doesn't contain a '+' query word, then the node is excluded
                        //
                        if(lword.StartsWith("+")) {
                            if(sword != "" && !name.Contains(sword)) {
                                included = false;
                                allWordsStartWithMin = false;
                                break;
                            }
                        }
                        //
                        // If a node name contains a '-' query word, then the node is excluded
                        //
                        else if (lword.StartsWith("-")) {
                            if (sword != "" && name.Contains(sword)) {
                                included = false;
                                allWordsStartWithMin = false;
                                break;
                            }
                        } else {
                            if (name.Contains(lword)) {
                                included = true;
                            }
                        }
                    }
                    if (included || allWordsStartWithMin) {
                        node.IsVisible = true;
                    }
                    else {
                        foreach (var link in node.Links) {
                            // Deze constructie is makkelijker omdat we een link al invisible mogen maken als 1 van de 2 nodes niet visible is.
                            // Ook als de buur node nog niet ge-evalueerd is in deze loop.
                            // De nodes daarentegen hebben de omgekeerde constructie omdat tussen de filter woorden een OR bestaat.
                            link.IsVisible = false;
                        }
                    }
                }
            }
            if (IsLoaded) {
                foreach (var node in Diagram.Nodes) {
                    nodeToUmlClassControl[(UmlClass) node].Visibility = !shouldFilter || node.IsVisible
                                                                      ? Visibility.Visible
                                                                      : Visibility.Collapsed;
                }
            }
        }

        #endregion

        #endregion

        #region Private methods

        #region Simulation pause / resume

        private void SubscribeForSimulationStateChanged(Graph diagram) {
            diagram.SuspendSimulation += OnSuspendSimulation;
            diagram.ResumeSimulation += OnResumeSimulation;
        }

        private void OnSuspendSimulation(object sender, EventArgs e) {
            IsSimulating = false;
        }

        private void OnResumeSimulation(object sender, EventArgs e) {
            IsSimulating = true;
        }

        #endregion
        
        #region Enter / exit Edit Mode

        private void SubscribeForEditModeChanged(Graph diagram) {
            diagram.EnterEditMode += OnEnterEditMode;
            diagram.ExitEditMode += OnExitEditMode;
        }

        private void OnEnterEditMode(object sender, EventArgs e) {
            IsInEditMode = true;
        }

        private void OnExitEditMode(object sender, EventArgs e) {
            IsInEditMode = false;
        }

        #endregion

        #region Sync nodes

        private void SubscribeForNodesChanged(Graph diagram) {
            diagram.Nodes.CollectionChanged += OnNodesChanged;
        }

        private void SubscribeForNodeOverlayChanged(Graph diagram) {
            diagram.ShowNodeOverlay += (sender, args) => SetShowsNodeOverlay(this, true);
            diagram.HideNodeOverlay += (sender, args) => SetShowsNodeOverlay(this, false);
        }

        private void InitNodes() {
            Loaded += (sender, args) => {
                          foreach (var node in Diagram.Nodes) {
                              AddNode(node);
                          }
                      };
            OnFilterTextChanged(null, null);
        }

        private void OnNodesChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if(e.Action == NotifyCollectionChangedAction.Reset) {
                Children.Clear();
                nodeToUmlClassControl.Clear();
                umlClassControlToNode.Clear();
                NodeNames.Clear();
                return;
            }
            if (e.OldItems != null) {
                foreach (Node node in e.OldItems) {
                    var umlClassControl = nodeToUmlClassControl[(UmlClass) node];
                    Children.Remove(umlClassControl);
                    nodeToUmlClassControl.Remove((UmlClass)node);
                    umlClassControlToNode.Remove(umlClassControl);
                    NodeNames.Remove(node.Name);
                }
            }
            if (e.NewItems != null) {
                foreach (Node node in e.NewItems) {
                    AddNode(node);
                    if(!NodeNames.Contains(node.Name)) {
                        NodeNames.Add(node.Name);
                    }
                }
            }
            OnFilterTextChanged(null, FilterText);
        }

        private void AddNode(object node) {
            //var nodeContainer = new NodeContainer((UmlClass)node);
            var umlClassControl = new UmlClassControl((UmlClass)node);
            //nodeContainer.Child = umlClassControl;
            nodeToUmlClassControl[(UmlClass)node] = umlClassControl;
            umlClassControlToNode[umlClassControl] = (UmlClass)node;
            Children.Add(umlClassControl);
        }

        #endregion

        private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
            // TODO: we kunnen natuurlijk niet meer uitgaan van de size van de canvas, want deze wordt ook
            // getransformed enzo.
            Diagram.CenterViewport = new Point(((FrameworkElement)Parent).ActualWidth / 2, ((FrameworkElement)Parent).ActualHeight / 2);
        }

        #region Mouse events

        public void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var pos = e.GetPosition(this);
            startDraggingPoint = pos;
            e.Handled = Diagram.HandleMouseLeftButtonDown(pos);
            //if (mouseDownOnEmptySpace) {
            //    foreach (var container in nodeToContainer.Values) {
            //        var umlClassControl = container.Child as UmlClassControl;
            //        if (umlClassControl != null) {
            //            umlClassControl.IsCurrentlyEdited = false;
            //        }
            //    }
            //}
            isMouseLeftButtonDown = true;
        }

        
        public void OnMouseMove(object sender, MouseEventArgs e) {
            if (isMouseLeftButtonDown || isMouseRightButtonDown) {
                var pos = e.GetPosition(this);
                //Debug.WriteLine("MousePosition on canvas: " + pos.ToString());
                e.Handled = Diagram.HandleMouseMove(pos, startDraggingPoint);                
            }         
        }

        public void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var pos = e.GetPosition(this);
            if(isMouseLeftButtonDown) {
                e.Handled = Diagram.HandleMouseLeftButtonUp(pos);
                isMouseLeftButtonDown = false;
            }
        }

        public void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
        }

        public void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var pos = e.GetPosition(this);
            if (isMouseRightButtonDown) {
                e.Handled = Diagram.HandleMouseRightButtonUp(pos);
                isMouseRightButtonDown = false;
            }
        }

        private static void OnMouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }

        private void OnMouseLeftButtonUpOnNodeBody(object sender, RoutedEventArgs e) {
            var classControl = (UmlClassControl)e.Source;
            var node = umlClassControlToNode[classControl];
            if (isMouseLeftButtonDown) {
                Diagram.HandleMouseLeftButtonUpOnNodeBody(node);
                isMouseLeftButtonDown = false;
            }
        }

        private void OnMouseLeftButtonUpOnInheritanceConnector(object sender, RoutedEventArgs e) {
            var classControl = (UmlClassControl)e.Source;
            var node = umlClassControlToNode[classControl];
            if (isMouseLeftButtonDown) {
                Diagram.HandleMouseLeftButtonUpOnInheritanceConnector(node);
                isMouseLeftButtonDown = false;
            }
        }

        private void OnMouseLeftButtonUpOnCompositionConnector(object sender, RoutedEventArgs e) {
            var classControl = (UmlClassControl)e.Source;
            var node = umlClassControlToNode[classControl];
            if (isMouseLeftButtonDown) {
                Diagram.HandleMouseLeftButtonUpOnCompositionConnector(node);
                isMouseLeftButtonDown = false;
            }
        }

        private void OnMouseLeftButtonUpOnAssociationConnector(object sender, RoutedEventArgs e) {
            var classControl = (UmlClassControl)e.Source;
            var node = umlClassControlToNode[classControl];
            if (isMouseLeftButtonDown) {
                Diagram.HandleMouseLeftButtonUpOnAssociationConnector(node);
                isMouseLeftButtonDown = false;
            }
        }


        private void OnMouseWheel(object sender, MouseWheelEventArgs e) {
            //var pos = e.GetPosition(this);

        }

        #endregion

        #region Drawing

        private void OnCompositionTargetRendering(object sender, EventArgs e) {
            // Gebruik floats ipv doubles, veel sneller:
            // https://evanl.wordpress.com/category/graphics/
            //Debug.WriteLine("OnCompositionTargetRendering: " + Thread.CurrentThread.ManagedThreadId);
            foreach (var child in Children) {
                var umlClassControl = (UmlClassControl) child;
                umlClassControl.Update();
            }
            UpdateLayout();
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc) {
            //Debug.WriteLine("OnRender: " + Thread.CurrentThread.ManagedThreadId);
            base.OnRender(dc);
            var now = DateTime.Now;
            if (started) {
                TimeSpan deltaTime = now - previousTime;
                SimulateStep(deltaTime, IsSimulating || simulateOneTimeStep, dc);
            }
            started = true;
            previousTime = now;
            simulateOneTimeStep = false;
        }

        private void SimulateStep(TimeSpan timeStep, bool isSimulating, DrawingContext dc) {
            TimeSpan deltaTime = timeStep;
            CalculateFps(deltaTime);
            double previousKineticEnergy = KineticEnergy;
            if (double.IsNaN(previousKineticEnergy) || previousKineticEnergy < 0.1) {
                previousKineticEnergy = 100000.0;
            }
            KineticEnergy =
                Simulator.Simulate(
                    deltaTime.TotalSeconds * TimeSpeedFactor/* * ((Fps == 0 ? 60 : Fps) / 60.0)/* * 100000.0 / previousKineticEnergy*/,
                    ((FrameworkElement)Parent).ActualWidth,
                    ((FrameworkElement)Parent).ActualHeight,
                    isSimulating
                );

            DrawLinks(dc);
            if (DrawsForces) {
                DrawForces(dc);
            }
        }

        private void CalculateFps(TimeSpan deltaTime) {
            fpsTime += deltaTime.TotalMilliseconds;
            fpsCount++;
            if (fpsTime > 1000) {
                fpsTime = 0;
                lastFpsCount = fpsCount;
                fpsCount = 0;
            }
            Fps = lastFpsCount;
        }

        private void DrawLinks(DrawingContext dc) {
            foreach (var link in Diagram.Links.Where(n => n.IsVisible)) {
                link.Draw(dc);
            }
        }

        private void DrawForces(DrawingContext dc) {
            foreach (var node in Diagram.Nodes.Where(n => n.IsVisible)) {
                dc.PushOpacity(0.5);
                DrawForce(node, ForceType.Repulsion, dc, whitePen);
                DrawForce(node, ForceType.Attraction, dc, greenPen);
                DrawForce(node, ForceType.DiscreteAngles, dc, bluePen);
                DrawForce(node, ForceType.NeighbourConnectorRepulsion, dc, redPen);
                DrawForce(node, ForceType.Node2LinkRepulsion, dc, brownPen);
                dc.Pop();
                DrawTotalForce(node, dc, blackPen);
            }
        }

        private void DrawTotalForce(Node node, DrawingContext dc, Pen pen) {
            //Vector3D force = new Vector3D(0,0,0);
            //foreach (var value in node.Forces.Values) {
            //    force += value;
            //}
            DrawForce(node, node.Force, dc, pen);
        }

        private void DrawForce(Node node, ForceType type, DrawingContext dc, Pen pen) {
            if (!node.Forces.ContainsKey(type)) {
                return;
            }
            var force = node.Forces[type];
            DrawForce(node, force, dc, pen);
        }

        private void DrawForce(Node node, Vector3D force, DrawingContext dc, Pen pen) {
            var pos0 = node.Pos2D + offsetVector;
            var vec1 = new Vector(force.X, force.Y);
            var vec2 = vec1 * rotation45Matrix;
            var vec3 = vec1 * rotationMin45Matrix;
            vec2.Normalize();
            vec3.Normalize();
            var pos4 = pos0 + vec1 * 10;
            var pos5 = pos4 - vec2 * 4;
            var pos6 = pos4 - vec3 * 4;

            dc.DrawLine(pen, pos0, pos4);
            dc.DrawLine(pen, pos4, pos5);
            dc.DrawLine(pen, pos4, pos6);
        }

        #endregion

        private void OnExecuteStepCommand() {
            simulateOneTimeStep = true;
        }

        private void OnExecuteCopyDiagramToClipboardCommand() {
            Clipboard.Clear();
            Clipboard.SetText(totalStringBuilder.ToString());
        }

        #endregion
    }
}
