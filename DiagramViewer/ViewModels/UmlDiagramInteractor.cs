using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DiagramViewer.ViewModels {

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

    public class UmlDiagramInteractor {
        private readonly Diagram diagram;
        private MouseOperation currentMouseOperation = MouseOperation.None;
        private Vector dragOffsetVector;

        private DragOperation currentOperation = DragOperation.None;
        public DragOperation CurrentOperation {
            get { return currentOperation; }
            set {
// ReSharper disable RedundantCheckBeforeAssignment
                if (currentOperation != value) {
// ReSharper restore RedundantCheckBeforeAssignment
                    currentOperation = value;
                    //if (!IsSimulating && (value == DragOperation.None || value == DragOperation.MovingNode)) {
                    //    //IsSimulating = true;
                    //} else if (IsSimulating && (value != DragOperation.None && value != DragOperation.MovingNode)) {
                    //    //
                    //    // When creating a node or link => stop the simulation
                    //    //
                    //    //IsSimulating = false;
                    //}
                }
            }
        }

        protected DiagramNode ActiveNode { get; set; }
        protected UmlDiagramRelation ActiveRelation { get; set; }

        public UmlDiagramInteractor(Diagram diagram) {
            this.diagram = diagram;
        }

        #region Mouse events

        public bool HandleMouseLeftButtonDown(Point point) {
            DiagramNode hoveredNode = GetHoveredNode(point, out dragOffsetVector);
            if (hoveredNode != null) {
                currentMouseOperation = MouseOperation.MouseLeftButtonDownOnNode;
                ActiveNode = hoveredNode;
                ActiveNode.IsPositionControlled = true;
                return OnMouseLeftButtonDownOnNodeOverride(point);
            }
            currentMouseOperation = MouseOperation.MouseLeftButtonDownOnEmptySpace;
            return OnMouseLeftButtonDownOnEmptySpaceOverride(point);
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
            if (ActiveNode != null) {
                ActiveNode.IsPositionControlled = false;
            }
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

        private bool OnMouseLeftButtonDownOnNodeOverride(Point point) { return true; }

        private bool OnMouseLeftButtonDownOnEmptySpaceOverride(Point point) { return false; }

        // -----------------------------------------------------------------------------

        protected virtual bool OnLeftDraggingOnNode(Point point, Point startingPoint) {
            // move node
            CurrentOperation = DragOperation.MovingNode;
            var vector = point - startingPoint;
            ActiveNode.Pos = ActiveNode.Pos + vector;
            //ActiveNode.IsBeingDragged = true;
            return true;
        }

        protected virtual bool OnLeftDraggingOnEmptySpace(Point point, Point startingPoint) {
            return false;
        }

        // -----------------------------------------------------------------------------

        protected virtual void OnMouseMove(Point point, Point startingPoint) {
            if (CurrentOperation == DragOperation.MovingNode) {
                ActiveNode.Pos = point - dragOffsetVector;
            }
        }

        // -----------------------------------------------------------------------------

        //protected virtual void OnMouseLeftButtonUpOnNode(Node node) {
        //    if (ActiveNode != null) {
        //        ActiveNode.IsBeingDragged = false;
        //    }
        //    if (ActiveRelation != null) {
        //        ActiveRelation.Node2 = node;
        //        node.Links.Add(ActiveRelation);
        //        ActiveRelation.IsBeingDragged = false;
        //        ActiveRelation = null;
        //    }
        //    CurrentOperation = DragOperation.None;
        //}

        protected virtual bool OnMouseLeftButtonUpOnEmptySpace() {
            //if (ActiveNode != null) {
            //    //ActiveNode.IsBeingDragged = false;
            //}
            //if (ActiveRelation != null) {
            //    ActiveRelation.StartNode.Links.Remove(ActiveRelation);
            //    Links.Remove(ActiveRelation);
            //    ActiveRelation = null;
            //}
            CurrentOperation = DragOperation.None;
            return false;
        }

        #endregion

        private DiagramNode GetHoveredNode(Point mousePosition, out Vector offset) {
            foreach (var diagramNode in diagram.Nodes) {
                if (diagramNode.ContainsPoint(mousePosition)) {
                    offset = mousePosition - diagramNode.Pos;
                    return diagramNode;
                }
            }
            offset = new Vector(0, 0);
            return null;
        }

        public void UpdateContentSize() {
            //
            // Meet de bounding box van de content in CC.
            //

            // OK. UmlCanvas krijg de mouse events binnen voor de nodes. Als ik bv een node wil verplaatsen dan moet ik overal op het scherm mousevents kunnen ontvangen.
            // Dit betekent meteen al dat UmlCanvas altijd de hele viewport moet bedekken.
            // Tegelijkertijd moet UmlCanvas kleiner gemaakt kunnen worden om te zoomen. Als de scalefactor groter dan 1 wordt moet UmlCanvas dus zijn width en height aanpassen
            // en daarnaast mag de viewport nooit een negatieve offset hebben tov de canvas.
            // Dit wordt wel allemaal heel moeilijk. Wat logischer is, is dat ik de mouse events gewoon in de zoomandpancontrol ontvang en deze door delegeer naar het canvas.
            // Als je dit doet, hoeft het canvas ook niet meer te rescalen want hij staat children toe om buiten zijn grenzen te komen.

            //if (!first) return;
            //first = false;

            List<DiagramNode> forceExertingNodes = diagram.ForceExertingNodes.ToList();
            if(forceExertingNodes.Count == 0) return;
            Rect contentRect = new Rect(forceExertingNodes[0].TopLeft, forceExertingNodes[0].BottomRight);


            foreach (var diagramNode in forceExertingNodes.Skip(1)) {
                //var umlClass = diagramListBoxItem.UmlClass;
                //if (umlClass != null && umlClass.IsVisible) {
                // contentRect contains all node containers and is as small as possible
                // UmlCanvas will get the same size as this contentRect because it binds to ZAPVM.ContentWidth and ZAPVM.ContentHeight
                contentRect.Union(new Rect(diagramNode.TopLeft, diagramNode.Size));
                //}
            }
            //Debug.WriteLine(string.Format("Content bounding box, (x, y, width, height): ({0:f0},{1:f0},{2:f0},{3:f0})", contentRect.X, contentRect.Y, contentRect.Width, contentRect.Height));
            //
            // TODO: als je dit doet terwijl je een node aan het verplaatsen bent, krijg je:
            // 1. Verplaats node naar binnen
            // 2. UmlCanvas verandert van size
            // 3. Omdat de size verandert, verandert de e.GePosition(canvas) ook
            // De size mag natuurlijk niet veranderen als we in een drag situatie zit.
            // OF, we vangen de mouse positions niet af op het canvas.
            //
            if (CurrentOperation == DragOperation.None) {
                diagram.ZoomAndPanViewModel.ContentWidth = contentRect.Width;
                diagram.ZoomAndPanViewModel.ContentHeight = contentRect.Height;
                //Debug.WriteLine(contentRect.Width + " " + contentRect.Height);
            }
        }
    }
}
