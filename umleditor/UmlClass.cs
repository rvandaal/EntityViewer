using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using GraphFramework;

namespace UmlEditor {
    public class UmlClass : Rectangle {

        private int numberOfInheritors = -1;
        public UmlClass(Graph graph) : base(graph) {
            Links.CollectionChanged += OnLinksChanged;
            PropertyChanged += OnPropertyChanged;
            ForegroundBrush = Brushes.Black;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == "Name" && (BackgroundBrush == null || ((SolidColorBrush)BackgroundBrush).Color == Colors.Beige)) {
                var name = Name.ToLower();
                if (name.Contains("usercontrol") || name =="mainwindow") {
                    BackgroundBrush = Brushes.DeepSkyBlue;
                } else if(name == "session") {
                    BackgroundBrush = Brushes.CornflowerBlue;
                } else if (name.Contains("viewmodel")) {
                    BackgroundBrush = Brushes.Yellow;
                } else if (name.Contains("datacontroller")) {
                    BackgroundBrush = Brushes.DarkOrange;
                } else if (name.Contains("controller")) {
                    BackgroundBrush = Brushes.Red;
                } else if(name.Contains("presentationhandler")) {
                    BackgroundBrush = Brushes.YellowGreen;
                } else if(name.Contains("presentationstate")) {
                    BackgroundBrush = Brushes.MediumSeaGreen;
                } else if (name.Contains("presentation")) {
                    BackgroundBrush = Brushes.GreenYellow;
                } else if (name.Contains("scenehandler")) {
                    BackgroundBrush = Brushes.LightSeaGreen;
                } else {
                    BackgroundBrush = Brushes.Beige;
                }
            }
        }

        private void OnLinksChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if(e.NewItems != null) {
                UpdateBackgroundBrush();
            }
        }

        private void UpdateBackgroundBrush() {
            if (BackgroundBrush == null) {
                foreach (var link in Links.Where(l => l.StartNode == this && (l is UmlInheritanceRelation || l is UmlCompositionRelation))) {
                    var otherNode = link.GetNeighbourNode(this) as UmlClass;
                    if (otherNode != null && ((SolidColorBrush)otherNode.BackgroundBrush).Color != Colors.Beige) {
                        BackgroundBrush = otherNode.BackgroundBrush;
                    }
                }
            }
            if (BackgroundBrush != null) {
                foreach (var link in Links.Where(l => l.EndNode == this && (l is UmlInheritanceRelation || l is UmlCompositionRelation))) {
                    var otherNode = link.GetNeighbourNode(this) as UmlClass;
                    if (otherNode != null && ((SolidColorBrush)otherNode.BackgroundBrush).Color == Colors.Beige) {
                        otherNode.BackgroundBrush = backgroundBrush;
                    }
                }
            }
        }

        private Brush backgroundBrush;
        public Brush BackgroundBrush {
            get { return backgroundBrush; }
            set {
                if(backgroundBrush != value) {
                    backgroundBrush = value;
                    UpdateBackgroundBrush();
                    if (value is SolidColorBrush &&
                        (((SolidColorBrush)value).Color == Colors.Red || ((SolidColorBrush)value).Color == Colors.DarkOrange || ((SolidColorBrush)value).Color == Colors.CornflowerBlue)) {
                        ForegroundBrush = Brushes.White;
                    }
                    OnPropertyChanged("BackgroundBrush");
                }
            }
        }

        private Brush foregroundBrush;
        public Brush ForegroundBrush {
            get { return foregroundBrush; }
            set {
                if (foregroundBrush != value) {
                    foregroundBrush = value;
                    OnPropertyChanged("ForegroundBrush");
                }
            }
        }

        private readonly ObservableCollection<UmlAttribute> attributes = new ObservableCollection<UmlAttribute>();
        public ObservableCollection<UmlAttribute> Attributes {
            get { return attributes; }
        }

        private readonly ObservableCollection<UmlOperation> operations = new ObservableCollection<UmlOperation>();
        public ObservableCollection<UmlOperation> Operations {
            get { return operations; }
        } 

        public int NumberOfInheritors {
            get {
                if (numberOfInheritors == -1) {
                    int no = 0;
                    foreach (var link in Links.OfType<UmlInheritanceRelation>().Where(l => l.EndNode == this)) {
                        var otherNode = link.StartNode as UmlClass;
                        Debug.Assert(otherNode != null);
                        no += 1 + otherNode.NumberOfInheritors;
                    }
                    numberOfInheritors = no;
                }
                return numberOfInheritors;
            }
        }

        public Assembly Assembly { get; set; }

        public override void Draw(DrawingContext dc) {
            dc.DrawRectangle(Utils.NodeBrush, Utils.DefaultPen, new Rect(TopLeft2D, BottomRight2D));
            var height = (BottomLeft2D - TopLeft2D).Length;
            var offset = new Vector(0, Math.Min(height, 15));
            dc.DrawLine(Utils.DefaultPen, TopLeft2D + offset, TopRight2D + offset);
        }

        private double mass;
        public override double Mass {
            get {
                if (mass < 0.01) {
                    mass = Math.Max(1, (double)NumberOfInheritors);
                }
                return mass;
            }
        }

        public override double GetForceMultiplier(ForceType forceType) {
            if(forceType == ForceType.DiscreteAngles) {
                return NumberOfInheritors;
            }
            return 1.0;
        }
    }
}
