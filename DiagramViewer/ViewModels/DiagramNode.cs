using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using DiagramViewer.Utilities;

namespace DiagramViewer.ViewModels {
    public class DiagramNode : ViewModelBase {

        public ObservableCollection<string> Tags { get; private set; }

        public DiagramNode() {
            isVisible = true;
            exertsForces = true;
            acceptsForces = true;
            isPositionControlled = false;
            ForceMultiplier = 1.0;
            Opacity = 1.0;
            Size = new Size(100, 50);

            links = new ObservableCollection<DiagramLink>();
            Links = new ReadOnlyObservableCollection<DiagramLink>(links);

            Tags = new ObservableCollection<string>();
        }

        public ReadOnlyObservableCollection<DiagramLink> Links { get; private set; }
        private readonly ObservableCollection<DiagramLink> links;

        public void AddLink(DiagramLink diagramLink) {
            if (!links.Contains(diagramLink)) {
                links.Add(diagramLink);
            }
        }

        public void RemoveLink(DiagramLink diagramLink) {
            if (links.Contains(diagramLink)) {
                links.Remove(diagramLink);
            }
        }

        public void AddTag(string tag) {
            if (!Tags.Contains(tag)) {
                Tags.Add(tag);
            }
        }

        public void RemoveTag(string tag) {
            if (Tags.Contains(tag)) {
                Tags.Remove(tag);
            }
        }

        private bool isVisible;
        public bool IsVisible {
            get { return true; }
            set {
                if (SetProperty(value, ref isVisible, () => IsVisible)) {
                    if (CanAnimate) {
                        if (value) {
                            IsFadingInOpacity = true;
                            IsFadingOutOpacity = false;
                        } else {
                            IsFadingInOpacity = false;
                            IsFadingOutOpacity = true;
                        }
                        AnimationCounterOpacity = 0.0;
                    } else {
                        Opacity = value ? 1.0 : 0.0;
                    }
                }
            }
        }

        private double opacity;
        public double Opacity {
            get { return opacity; }
            set { SetDoubleProperty(value, ref opacity, () => Opacity); }
        }

        private bool acceptsForces;
        public bool AcceptsForces {
            get { return acceptsForces; }
            set { SetProperty(value, ref acceptsForces, () => AcceptsForces); }
        }

        private bool exertsForces;
        public bool ExertsForces {
            get { return exertsForces; }
            set {
                if (SetProperty(value, ref exertsForces, () => ExertsForces)) {
                    if (CanAnimate) {
                        if (value) {
                            IsFadingIn = true;
                            IsFadingOut = false;
                        } else {
                            IsFadingIn = false;
                            IsFadingOut = true;
                        }
                        AnimationCounter = 0.0;
                    } else {
                        ForceMultiplier = value ? 1.0 : 0.0;
                    }
                }
            }
        }

        private bool isPositionControlled;
        public bool IsPositionControlled {
            get { return isPositionControlled; }
            set { SetProperty(value, ref isPositionControlled, () => IsPositionControlled); }
        }

        private bool canAnimate;
        public bool CanAnimate {
            get { return canAnimate; }
            set {
                if (canAnimate != value) {
                    canAnimate = value;
                    if (IsFadingIn || IsFadingOut) {
                        animationCounterOpacity = 0.0;
                    }
                }
            }
        }

        public double ForceMultiplier { get; set; }
        public bool IsFadingIn { get; set; }
        public bool IsFadingOut { get; set; }

        public double ForceMultiplierOpacity { get; set; }
        public bool IsFadingInOpacity { get; set; }
        public bool IsFadingOutOpacity { get; set; }

        private double animationCounter;
        public double AnimationCounter {
            get { return animationCounter; }
            set {
                if (animationCounter != value) {
                    animationCounter = value;
                    if (value > 2.0) {
                        ForceMultiplier = IsFadingIn ? 1.0 : 0.0;
                        IsFadingIn = false;
                        IsFadingOut = false;
                        animationCounter = 0.0;
                    }
                }
            }
        }

        private double animationCounterOpacity;
        public double AnimationCounterOpacity {
            get { return animationCounterOpacity; }
            set {
                if (animationCounterOpacity != value) {
                    animationCounterOpacity = value;
                    if (value > 2.0) {
                        Opacity = IsFadingInOpacity ? 1.0 : 0.0;
                        IsFadingInOpacity = false;
                        IsFadingOutOpacity = false;
                        animationCounterOpacity = 0.0;
                    }
                }
            }
        }

        public void UpdateForceMultiplier(double dt) {
            if (IsFadingIn || IsFadingOut) {
                AnimationCounter += dt;
                if (IsFadingIn) {
                    ForceMultiplier = AnimationCounter / 2.0;
                } else if (IsFadingOut) {
                    ForceMultiplier = 1 - AnimationCounter / 2.0;
                }
            }

            if (IsFadingInOpacity || IsFadingOutOpacity) {
                AnimationCounterOpacity += dt;
                if (IsFadingInOpacity) {
                    Opacity = AnimationCounterOpacity / 2.0;
                } else if (IsFadingOutOpacity) {
                    //
                    // Er lijkt hier sprake van een race condition te zijn:
                    // we komen in dit blok dus IsFadingIn of IsFadingOut is true
                    // Maar toch zijn deze eerder allebei op false gezet.
                    // Daarom geen else hier maar een else if.
                    //
                    Opacity = 1 - AnimationCounterOpacity / 2.0;
                }
            }
        }

        public virtual double Mass { get { return 1.0; } }

        private Size size;
        public Size Size {
            get { return size; }
            set {
                if (SetProperty(value, ref size, () => Size)) {
                    OnSizeChanged();
                }
            }
        }

        public Vector Force {
            get {
                Vector totalForce = new Vector(0, 0);
                foreach (var force in Forces.Values) {
                    totalForce += force;
                }
                return totalForce;
            }
        }

        private readonly Dictionary<ForceType, Vector> forces = new Dictionary<ForceType, Vector>();
        public Dictionary<ForceType, Vector> Forces {
            get { return forces; }
        }

        public void ResetForces() {
            foreach (ForceType forceType in Enum.GetValues(typeof(ForceType))) {
                Forces[forceType] = new Vector(0, 0);
            }
        }

        public void AddForce(ForceType forceType, Vector force) {
            Forces[forceType] += force;
        }

        private Vector acc;
        public Vector Acc {
            get { return acc; }
            set { SetProperty(value, ref acc, () => Acc); }
        }

        private Vector vel;
        public Vector Vel {
            get { return vel; }
            set { SetProperty(value, ref vel, () => Vel); }
        }

        private Point pos;
        public Point Pos {
            get { return pos; }
            set {
                if (SetProperty(value, ref pos, () => Pos)) {
                    OnPosChanged();
                    NotifyPropertyChanged(() => TopLeftPos);
                }
            }
        }

        public Point TopLeftPos { get { return TopLeft; } }

        public Point TopLeft { get; private set; }
        public Point TopRight { get; private set; }
        public Point BottomRight { get; private set; }
        public Point BottomLeft { get; private set; }

        protected virtual void OnPosChanged() {
            UpdateProjectedCornerPositions();
        }

        protected virtual void OnSizeChanged() {
            UpdateProjectedCornerPositions();
        }

        private void UpdateProjectedCornerPositions() {
            var left = Pos.X - Size.Width / 2;
            var right = Pos.X + Size.Width / 2;
            var top = Pos.Y - Size.Height / 2;
            var bottom = Pos.Y + Size.Height / 2;

            TopLeft = new Point(left, top);
            TopRight = new Point(right, top);
            BottomRight = new Point(right, bottom);
            BottomLeft = new Point(left, bottom);
        }

        public virtual double GetDistanceToNode(DiagramNode diagramNode) {

            return (Pos - diagramNode.Pos).Length;

            //return Math.Min(
            //    Math.Min(
            //        Math.Min(
            //            diagramNode.GetShortestDistanceToPoint(TopLeft),
            //            diagramNode.GetShortestDistanceToPoint(TopRight)
            //        ),
            //        diagramNode.GetShortestDistanceToPoint(BottomRight)
            //    ),
            //    diagramNode.GetShortestDistanceToPoint(BottomLeft)
            //);
        }

        public virtual double GetShortestDistanceToPoint(Point point) {

            // Corners first, biggest area
            if (point.X < TopLeft.X && point.Y < TopLeft.Y) {
                return GetDistanceBetweenPoints(TopLeft, point);
            }
            if (point.X > TopRight.X && point.Y < TopRight.Y) {
                return GetDistanceBetweenPoints(TopRight, point);
            }
            if (point.X > BottomRight.X && point.Y > BottomRight.Y) {
                return GetDistanceBetweenPoints(BottomRight, point);
            }
            if (point.X < BottomLeft.X && point.Y > BottomLeft.Y) {
                return GetDistanceBetweenPoints(BottomLeft, point);
            }

            // Edges
            if (point.X < TopLeft.X) {
                return TopLeft.X - point.X;
            }
            if (point.Y < TopLeft.Y) {
                return TopLeft.Y - point.Y;
            }
            if (point.X > TopRight.X) {
                return point.X - TopRight.X;
            }
            if (point.Y > BottomLeft.Y) {
                return point.Y - BottomLeft.Y;
            }
            return 0;
        }

        protected static double GetDistanceBetweenPoints(Point a, Point b) {
            return (a - b).Length;
        }

        public virtual double GetDistanceOfLinkToEdge(DiagramLink diagramLink) {
            var bottomRightCornerAngle = Vector.AngleBetween(Utils.Angle0Vector, BottomRight - Pos);
            var linkAngle = diagramLink.Angle0;
            if (Math.Abs(linkAngle) <= bottomRightCornerAngle || Math.Abs(linkAngle) >= 180 - bottomRightCornerAngle) {
                return Size.Width / 2 / Math.Abs(Math.Cos(linkAngle / 180 * Math.PI));
            }
            return Size.Height / 2 / Math.Sin(Math.Abs(linkAngle / 180 * Math.PI));
        }

        public bool ContainsPoint(Point point) {
            return point.X >= TopLeft.X &&
                   point.X <= BottomRight.X &&
                   point.Y >= TopLeft.Y &&
                   point.Y <= BottomRight.Y;
        }
    }
}
