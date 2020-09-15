using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Linq;
using System.Collections.ObjectModel;

namespace GraphFramework {

    public enum MouseOperation {
        None,
        MouseLeftButtonDownOnNode,
        MouseLeftButtonDownOnEmptySpace,
        MouseRightButtonDownOnNode,
        MouseRightButtonDownOnEmptySpace
    }

    public enum DragOperation {
        None,
        MovingNode,
        CreatingNode,
        CreatingLink
    }

    /// <summary>
    /// Graph containing nodes and links.
    /// </summary>
    /// <remarks>
    /// This class handles mouse events and thus is a view specific class, not a model class.
    /// Multiple Graphs per model can be made and Nodes and Links may contain a IsVisible property.
    /// Nodes also know their forces, positions, acc, vel, etc; these are all simulation (=view) specific
    /// properties.
    /// So for now, we don't have a model, we work with a view on a model.
    /// </remarks>
    public class Graph {

        public event EventHandler SuspendSimulation;
        public event EventHandler ResumeSimulation;
        public event EventHandler EnterEditMode;
        public event EventHandler ExitEditMode;
        public event EventHandler ShowNodeOverlay;
        public event EventHandler HideNodeOverlay;

        private bool isSimulating = true;
        public bool IsSimulating {
            get { return isSimulating; }
            set {
                if (isSimulating != value) {
                    isSimulating = value;
                    if (isSimulating) {
                        if (ResumeSimulation != null) {
                            ResumeSimulation(this, null);
                        }
                    } else {
                        if (SuspendSimulation != null) {
                            SuspendSimulation(this, null);
                        }
                    }
                }
            }
        }

        private bool isInEditMode;
        public bool IsInEditMode {
            get { return isInEditMode; }
            set {
                if (isInEditMode != value) {
                    isInEditMode = value;
                    if (isInEditMode) {
                        if (EnterEditMode != null) {
                            EnterEditMode(this, null);
                        }
                        IsSimulating = false;
                    } else {
                        if (ExitEditMode != null) {
                            ExitEditMode(this, null);
                        }
                    }
                }
            }
        }

        public Point CenterViewport { get; set; }

        internal Point ProjectPoint(Point3D point) {
            var point2D = new Point(point.X, point.Y);
            return CenterViewport + (point2D - CenterViewport) / point.Z;
        }

        internal Point3D UnprojectPoint(Point pos2D, double z) {
            var point = (pos2D - CenterViewport) * z + CenterViewport;
            return new Point3D(point.X, point.Y, z);
        }

        public Size ProjectSize(Size size, double z) {
            return new Size(size.Width / z, size.Height / z);
        }

        public Size UnprojectSize(Size size, double z) {
            return new Size(size.Width * z, size.Height * z);
        }

        #region Private fields

        private Vector dragOffsetVector;

        private MouseOperation currentMouseOperation = MouseOperation.None;

        private DragOperation currentOperation = DragOperation.None;
        public DragOperation CurrentOperation {
            get { return currentOperation; }
            set {
                if (currentOperation != value) {
                    currentOperation = value;
                    if (!IsSimulating && (value == DragOperation.None || value == DragOperation.MovingNode)) {
                        //IsSimulating = true;
                    } else if (IsSimulating && (value != DragOperation.None && value != DragOperation.MovingNode)) {
                        //
                        // When creating a node or link => stop the simulation
                        //
                        //IsSimulating = false;
                    }
                    if (value == DragOperation.CreatingLink) {
                        if (ShowNodeOverlay != null) {
                            ShowNodeOverlay(this, null);
                        }
                    } else {
                        if (HideNodeOverlay != null) {
                            HideNodeOverlay(this, null);
                        }
                    }
                }
            }
        }

        protected Node ActiveNode { get; set; }
        protected Link ActiveLink { get; set; }

        #endregion

        #region Public properties

        private ObservableCollection<Node> nodes = new ObservableCollection<Node>();
        public ObservableCollection<Node> Nodes {
            get { return nodes; }
        }

        public IEnumerable<Node> SimulatedNodes {
            get {
                return Nodes.Where(n => n.IsSimulated);
            }
        }

        public IEnumerable<Node> VisibleNodes {
            get {
                return Nodes.Where(n => n.IsVisible);
            }
        }

        private ObservableCollection<Link> links = new ObservableCollection<Link>();

        public ObservableCollection<Link> Links {
            get { return links; }
        }

        #endregion

        #region Public methods

        public virtual void Reset() {
            Nodes.Clear();
            Links.Clear();
            ActiveNode = null;
            ActiveLink = null;
        }

        public virtual void DeleteNode(Node node) {
            if (Nodes.Contains(node)) {
                Nodes.Remove(node);
                // use ToArray when removing links while iterating over them
                foreach(Link link in node.Links.ToArray()) { 
                    DeleteLink(link);
                }
            }
            if(ActiveNode == node) {
                ActiveNode = null;
            }
        }

        public virtual void DeleteLink(Link link) {
            if (Links.Contains(link)) {
                Links.Remove(link);
            }
            if (link.StartNode != null && link.StartNode.Links.Contains(link)) {
                link.StartNode.Links.Remove(link);
            }
            if (link.EndNode != null && link.EndNode.Links.Contains(link)) {
                link.EndNode.Links.Remove(link);
            }
            if(ActiveLink == link) {
                ActiveLink = null;
            }
        }

        #endregion

        #region Mouse events

        public bool HandleMouseLeftButtonDown(Point point) {
            Node hoveredNode = GetHoveredNode(point, out dragOffsetVector);
            if (hoveredNode != null) {                
                currentMouseOperation = MouseOperation.MouseLeftButtonDownOnNode;
                ActiveNode = hoveredNode;
                return OnMouseLeftButtonDownOnNodeOverride(point);
            }
            currentMouseOperation = MouseOperation.MouseLeftButtonDownOnEmptySpace;
            return OnMouseLeftButtonDownOnEmptySpaceOverride(point);
        }

        public bool HandleMouseRightButtonDown(Point point) {
            Node hoveredNode = GetHoveredNode(point, out dragOffsetVector);
            if (hoveredNode != null) {
                currentMouseOperation = MouseOperation.MouseRightButtonDownOnNode;
                ActiveNode = hoveredNode;
                return OnMouseRightButtonDownOnNodeOverride(point);
            }
            currentMouseOperation = MouseOperation.MouseRightButtonDownOnEmptySpace;
            return OnMouseRightButtonDownOnEmptySpaceOverride(point);
        }

        public bool HandleMouseMove(Point point, Point startingPoint) {
            // TODO: we cannot derive a delta vector here; ZoomAndPanControl should return this, since it 
            // applies a content scale factor
            bool handle = false;
            if (
                CurrentOperation == DragOperation.None &&
                (point - startingPoint).Length > 5
            ) {
                //
                // Start drag operation
                //
                switch (currentMouseOperation) {
                    case MouseOperation.MouseLeftButtonDownOnEmptySpace:
                        handle = OnLeftDraggingOnEmptySpace(point, startingPoint);
                        break;
                    case MouseOperation.MouseLeftButtonDownOnNode:
                        handle = OnLeftDraggingOnNode(point, startingPoint);
                        break;
                }
            }
            OnMouseMove(point, startingPoint);
            return handle;
        }

        public bool HandleMouseLeftButtonUp(Point point) {
            //Node hoveredNode = GetHoveredNode(point, out dragOffsetVector);
            //if (hoveredNode != null) {
            //    OnMouseLeftButtonUpOnNode(hoveredNode);
            //} else {
                bool handle = OnMouseLeftButtonUpOnEmptySpace();
            //}
            currentMouseOperation = MouseOperation.None;
            ActiveNode = null;
            return handle;
        }

        public bool HandleMouseRightButtonUp(Point point) {
            Node hoveredNode = GetHoveredNode(point, out dragOffsetVector);
            bool handle;
            handle = hoveredNode != null ? OnMouseRightButtonUpOnNodeOverride(hoveredNode) : OnMouseRightButtonUpOnEmptySpaceOverride();
            currentMouseOperation = MouseOperation.None;
            ActiveNode = null;
            return handle;
        }

        //public bool OnMouseRightButtonUp() {
        //    if (ActiveNode != null) {
        //        ActiveNode.IsBeingDragged = false;
        //        ActiveNode = null;
        //        return true;
        //    }
        //    return false;
        //}

        #endregion

        #region Protected methods

        protected virtual Node CreateNode() {
            return new Node(this);
        }

        protected virtual Link CreateLink() {
            return new Link();
        }

        //protected void Connect(Node node1, Node node2, double[] preferredAngles) {
        //    Links.Add(node1.ConnectTo(node2, preferredAngles));
        //}

        // -----------------------------------------------------------------------------
        protected virtual bool OnMouseLeftButtonDownOnNodeOverride(Point point) { return true; }

        protected virtual bool OnMouseLeftButtonDownOnEmptySpaceOverride(Point point) { return IsInEditMode; }

        protected virtual bool OnMouseRightButtonDownOnNodeOverride(Point point) {
            return true;
        }
        protected virtual bool OnMouseRightButtonDownOnEmptySpaceOverride(Point point) { return IsInEditMode; }

        // -----------------------------------------------------------------------------

        protected virtual bool OnLeftDraggingOnNode(Point point, Point startingPoint) {
            if (IsInEditMode) {
                CurrentOperation = DragOperation.CreatingLink;
                ActiveLink = CreateLink();
                ActiveLink.StartNode = ActiveNode;
                ActiveLink.IsBeingDragged = true;
                ActiveNode.Links.Add(ActiveLink);
                ActiveLink.ManualEndPoint = ActiveNode.Pos2D;
                Links.Add(ActiveLink);
            } else {
                // move node
                CurrentOperation = DragOperation.MovingNode;
                var vector = point - startingPoint;
                ActiveNode.Pos2D = ActiveNode.Pos2D + vector;
                ActiveNode.IsBeingDragged = true;
            }
            return true;
        }

        protected virtual bool OnLeftDraggingOnEmptySpace(Point point, Point startingPoint) {
            if (IsInEditMode) {
                CurrentOperation = DragOperation.CreatingNode;
                ActiveNode = CreateNode();
                ActiveNode.IsBeingDragged = true;
                ActiveNode.Pos = new Point3D(startingPoint.X, startingPoint.Y, 1);
                Nodes.Add(ActiveNode);
                return true;
            }
            return false;
        }

        // -----------------------------------------------------------------------------

        protected virtual void OnMouseMove(Point point, Point startingPoint) {
            if (CurrentOperation == DragOperation.MovingNode) {
                ActiveNode.Pos2D = point - dragOffsetVector;
            }
        }

        // -----------------------------------------------------------------------------

        //protected virtual void OnMouseLeftButtonUpOnNode(Node node) {
        //    if (ActiveNode != null) {
        //        ActiveNode.IsBeingDragged = false;
        //    }
        //    if (ActiveLink != null) {
        //        ActiveLink.Node2 = node;
        //        node.Links.Add(ActiveLink);
        //        ActiveLink.IsBeingDragged = false;
        //        ActiveLink = null;
        //    }
        //    CurrentOperation = DragOperation.None;
        //}

        protected virtual bool OnMouseLeftButtonUpOnEmptySpace() {
            if (ActiveNode != null) {
                ActiveNode.IsBeingDragged = false;
            }
            if (ActiveLink != null) {
                ActiveLink.StartNode.Links.Remove(ActiveLink);
                Links.Remove(ActiveLink);
                ActiveLink = null;
            }
            CurrentOperation = DragOperation.None;
            return false;
        }

        protected virtual bool OnMouseRightButtonUpOnNodeOverride(Node node) {
            Debug.Assert(ActiveLink == null);
            if (ActiveNode != null) {
                ActiveNode.IsBeingDragged = false;
                ActiveNode.Vel = new Vector3D(0, 0, 0);
                ActiveNode = null;
            }
            CurrentOperation = DragOperation.None;
            return false;
        }

        protected virtual bool OnMouseRightButtonUpOnEmptySpaceOverride() {
            Debug.Assert(ActiveLink == null);
            if (ActiveNode != null) {
                ActiveNode.IsBeingDragged = false;
                ActiveNode = null;
            }
            CurrentOperation = DragOperation.None;
            return false;
        }

        // -----------------------------------------------------------------------------

        #endregion

        #region Private methods

        private Node GetHoveredNode(Point mousePosition, out Vector offset) {
            foreach (var node in Nodes) {
                if (node.ContainsPoint2D(mousePosition)) {
                    offset = mousePosition - node.Pos2D;
                    return node;
                }
            }
            offset = new Vector(0, 0);
            return null;
        }

        #endregion
    }
}
