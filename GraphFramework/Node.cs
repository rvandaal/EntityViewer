using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System;
using System.Windows;

namespace GraphFramework {

    public class Node : INotifyPropertyChanged {

        public static int StaticIndex { get; set; }

        public int Index { get; set; }

        private string name;
        public string Name {
            get { return name; }
            set {
                if(name != value) {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public Node(Graph graph) {
            Index = StaticIndex++;
            Graph = graph;
            ResetForces();
        }

        public Graph Graph { get; set; }

        private readonly ObservableCollection<Link> links = new ObservableCollection<Link>();
        public ObservableCollection<Link> Links {
            get { return links; }
        }

        public Link ConnectTo(Node node, double[] preferredAngles) {
            var link = new Link();
            link.StartNode = this;
            link.EndNode = node;
            link.PreferredAngles = preferredAngles;
            Links.Add(link);
            node.Links.Add(link);
            return link;
        }

        private readonly Dictionary<ForceType, Vector3D> forces = new Dictionary<ForceType, Vector3D>();
        public Dictionary<ForceType, Vector3D> Forces {
            get { return forces; }
        }

        public void ResetForces() {
            foreach (ForceType forceType in Enum.GetValues(typeof(ForceType))) {
                Forces[forceType] = new Vector3D(0, 0, 0);
            }
        }

        public void AddForce(ForceType forceType, Vector3D force) {
            Forces[forceType] += force;
        }

        public virtual bool ContainsPoint2D(Point point) {
            return false;
        }

        public virtual double GetDistanceToNode(Node node) {
            return GetDistanceToPoint(node.Pos);
        }

        public virtual double GetDistanceToPoint(Point3D point) {
            return GetDistanceBetweenPoints(Pos, point);
        }

        protected static double GetDistanceBetweenPoints(Point3D a, Point3D b) {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            var dz = a.Z - b.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public virtual void Draw(DrawingContext dc) {
            dc.DrawEllipse(Utils.NodeBrush, null, Pos2D, 2, 2);
        }

        public bool IsSimulated { get { return !IsBeingDragged && IsVisible; } }
        public bool IsBeingDragged { get; set; }
        public bool IsVisible { get; set; }

        private Point3D pos;
        public Point3D Pos {
            get { return pos; }
            set {
                if (pos != value) {
                    pos = value;
                    OnPosChanged();
                }
            }
        }

        private Point pos2D;
        public Point Pos2D {
            get { return pos2D; }
            set {
                if (pos2D != value) {
                    pos2D = value;
                    Pos = Graph.UnprojectPoint(pos2D, Pos.Z);
                }
            }
        }

        public void UpdatePositions() {
            OnPosChanged();
        }

        protected virtual void OnPosChanged() {
            pos2D = Graph.ProjectPoint(pos);
        }

        public Vector3D Vel { get; set; }
        public Vector3D Acc { get; set; }

        public Vector3D Force {
            get {
                Vector3D totalForce = new Vector3D(0, 0, 0);
                foreach (var force in Forces.Values) {
                    totalForce += force;
                }
                totalForce = LimitForce(totalForce);
                return totalForce;
            }
        }

        public virtual double Mass { get { return 1.0; } }

        public virtual double GetForceMultiplier(ForceType forceType) {
            return 1.0;
        }

        private Vector3D LimitForce(Vector3D inputForce) {
            Vector3D forceVector = inputForce;
            var force = forceVector.Length;
            var newForce = Math.Min(1000, force);
            if (force > 0 && newForce < force) {
                forceVector.Normalize();
                return newForce * forceVector;
            }
            return inputForce;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }


    // Check: 
    // https://social.msdn.microsoft.com/Forums/vstudio/en-US/2d685fd9-971d-4e00-84c5-870ff20d639d/advanced-polymorphism-philosophy-is-using-generics-an-option?forum=csharpgeneral
    // for the discussion regarding generic types like the one below.

}
