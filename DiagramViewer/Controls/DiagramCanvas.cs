using DiagramViewer.ViewModels;
using DiagramViewer.Views;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ZoomAndPan;
using DiagramViewer.Utilities;

namespace DiagramViewer.Controls {
    /// <summary>
    /// Reponsibilities:
    /// 1. Create and position nodes for each DiagramNode
    /// 2. Drawing relations and force vectors
    /// 3. Provide trigger for simulation
    /// 4. Pass mouse events to Diagram
    /// </summary>
    public class DiagramCanvas : Canvas {

        private readonly Pen redPen = new Pen(new SolidColorBrush(Colors.Red), 1);
        private readonly Pen greenPen = new Pen(new SolidColorBrush(Colors.Green), 1);
        private readonly Pen bluePen = new Pen(new SolidColorBrush(Colors.Blue), 1);
        private readonly Pen brownPen = new Pen(new SolidColorBrush(Colors.Brown), 1);
        private readonly Pen whitePen = new Pen(new SolidColorBrush(Colors.White), 1);
        private readonly Pen blackPen = new Pen(new SolidColorBrush(Colors.Black), 1) { DashStyle = new DashStyle(new[] { 4.0, 4.0 }, 0.0) };

        private readonly double halfsqrt2 = Math.Sqrt(2) / 2;
        private readonly Matrix rotation45Matrix;
        private readonly Matrix rotationMin45Matrix;

        private bool started;
        private DateTime previousTime;

        private Diagram diagram;
        private ZoomAndPanControl zoomAndPanControl;

        public bool ShowForces {
            get { return (bool)GetValue(ShowForcesProperty); }
            set { SetValue(ShowForcesProperty, value); }
        }

        public static readonly DependencyProperty ShowForcesProperty = DependencyProperty.Register(
            "ShowForces", typeof(bool), typeof(DiagramCanvas), new PropertyMetadata(default(bool)));


        public DiagramCanvas() {
            Loaded += OnLoaded;
            DataContextChanged += OnDataContextChanged;
            rotation45Matrix = new Matrix(halfsqrt2, -halfsqrt2, halfsqrt2, halfsqrt2, 0, 0);
            rotationMin45Matrix = new Matrix(halfsqrt2, halfsqrt2, -halfsqrt2, halfsqrt2, 0, 0);
        }

        private void OnLoaded(object sender, EventArgs e) {
            //CompositionTarget.Rendering += OnCompositionTargetRendering;
            CompositionTargetEx.FrameUpdating += OnCompositionTargetRendering;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            diagram = (Diagram)DataContext;
            //
            // Instead of using a binding to ItemsSource, we listen to the CollectionChanged event.
            // So, this still has to happen in DiagramCanvas.
            //
            ((INotifyCollectionChanged)diagram.Nodes).CollectionChanged += OnNodesChanged;
            InitNodes();
        }

        private void InitNodes() {
            if (IsLoaded) {
                SyncNodes();
            } else {
                Loaded += (sender, args) => SyncNodes();
            }
        }

        private void SyncNodes() {
            Children.Clear();
            foreach (var diagramNode in diagram.Nodes) {
                AddNode(diagramNode);
            }
        }

        private void OnNodesChanged(object sender, NotifyCollectionChangedEventArgs e) {
            //
            // We don't use a ListBox and ListBoxItems anymore:
            // 1. Links are not part of the listbox but it should be possible to select them. Using the ListBox for the classes and something else for the links is not consistent.
            // 2. ListBox handles mouse events and adds to much functionality that we don't need. We only need the multiselect functionality
            //    The generation of items we will do below.
            //
            if (e.Action == NotifyCollectionChangedAction.Reset) {
                Children.Clear();
                return;
            }
            if (e.OldItems != null && Children.Count > 0) {
                foreach (DiagramNode diagramNode in e.OldItems) {
                    int i = 0;
                    ContentPresenter child = (ContentPresenter)Children[i++];
                    while (child.Content != diagramNode) {
                        child = (ContentPresenter)Children[i++];
                    }
                    Children.Remove(child);
                }
            }
            if (e.NewItems != null) {
                foreach (DiagramNode diagramNode in e.NewItems) {
                    AddNode(diagramNode);
                }
            }
        }

        private void AddNode(DiagramNode diagramNode) {
            ContentPresenter contentPresenter = new ContentPresenter();
            contentPresenter.Content = diagramNode;
            // ReSharper disable TooWideLocalVariableScope
            FrameworkElementFactory diagramNodeViewFactory = null;
            // ReSharper restore TooWideLocalVariableScope
            if (diagramNode is UmlDiagramClass) {
                diagramNodeViewFactory = new FrameworkElementFactory(typeof(UmlDiagramClassView));

            } else if (diagramNode is UmlDiagramNote) {
                diagramNodeViewFactory = new FrameworkElementFactory(typeof(UmlDiagramNoteView));
            }
            else if (diagramNode is UmlDiagramMethodNode) {
                diagramNodeViewFactory = new FrameworkElementFactory(typeof(UmlDiagramMethodNodeView));
            }
            if (diagramNodeViewFactory != null) {
                contentPresenter.ContentTemplate = new DataTemplate {
                    VisualTree = diagramNodeViewFactory
                };
                Children.Add(contentPresenter);
            }
        }

        private void OnCompositionTargetRendering(object sender, EventArgs e) {
            //
            // Gebruik floats ipv doubles, veel sneller:
            // https://evanl.wordpress.com/category/graphics/
            //
            foreach (var child in Children) {
                var contentPresenter = (ContentPresenter)child;
                var diagramNode = (DiagramNode)contentPresenter.Content;
                if (Utilities.DoubleUtility.AreClose(diagramNode.Opacity, 0.0)) {
                    contentPresenter.Visibility = Visibility.Collapsed;
                    continue;
                }
                contentPresenter.Visibility = Visibility.Visible;
                contentPresenter.Opacity = diagramNode.Opacity;
                SetLeft(contentPresenter, diagramNode.TopLeft.X);
                SetTop(contentPresenter, diagramNode.TopLeft.Y);
                //contentPresenter.MinWidth = diagramNode.Size.Width;
                //contentPresenter.MinHeight = diagramNode.Size.Height;
                if (contentPresenter.ActualWidth != diagramNode.Size.Width || contentPresenter.ActualHeight != diagramNode.Size.Height) {
                    diagramNode.Size = new Size(contentPresenter.ActualWidth, contentPresenter.ActualHeight);
                }

            }

            diagram.UmlDiagramInteractor.UpdateContentSize();

            // TODO: Why is UpdateLayout needed?
            UpdateLayout();
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            var now = DateTime.Now;
            if (started) {
                TimeSpan deltaTime = now - previousTime;
                SimulateStep(deltaTime, dc);
            }
            started = true;
            previousTime = now;
        }

        private void SimulateStep(TimeSpan timeStep, DrawingContext dc) {
            TimeSpan deltaTime = timeStep;
            if (zoomAndPanControl == null) {
                zoomAndPanControl = this.FindAncestor<ZoomAndPanControl>();
            }
            diagram.Simulate(
                Math.Min(deltaTime.TotalSeconds, 0.1),
                diagram.ZoomAndPanViewModel.ContentWidth,
                diagram.ZoomAndPanViewModel.ContentHeight
            );

            DrawLinks(dc);
            if (ShowForces) {
                DrawForces(dc);
            }
        }

        private void DrawLinks(DrawingContext dc) {
            foreach (var diagramLink in diagram.Links) {
                if (diagramLink.StartNode.IsVisible && diagramLink.EndNode.IsVisible) {
                    //double opacity = 0.3;// diagramLink.StartNode.Opacity*diagramLink.EndNode.Opacity;
                    //if (!DoubleUtility.AreClose(opacity, 0.0)) {
                    //dc.PushOpacity(opacity);
                    //
                    // Testje met opacity:
                    // als je show mer* doet en nadat ie stabiel is aan de class gaat draggen, dan:
                    // - dropt de FPS van 60 naar 30 als je opacity pusht
                    // - blijft fps op 60 als je geen opacity pusht
                    //
                    diagramLink.Draw(dc);
                    //dc.Pop();
                    //}
                }
            }
        }

        private void DrawForces(DrawingContext dc) {
            foreach (var node in diagram.Nodes.Where(n => n.IsVisible)) {
                dc.PushOpacity(0.5);
                DrawForce(node, ForceType.Repulsion, dc, redPen);
                DrawForce(node, ForceType.NodeAttraction, dc, bluePen);
                DrawForce(node, ForceType.DiscreteAngles, dc, greenPen);
                DrawForce(node, ForceType.NeighbourConnectorRepulsion, dc, redPen);
                DrawForce(node, ForceType.Node2LinkRepulsion, dc, brownPen);
                dc.Pop();
                DrawTotalForce(node, dc, blackPen);
            }
        }

        private void DrawTotalForce(DiagramNode node, DrawingContext dc, Pen pen) {
            //Vector3D force = new Vector3D(0,0,0);
            //foreach (var value in node.Forces.Values) {
            //    force += value;
            //}
            DrawForce(node, node.Force, dc, pen);
        }

        private void DrawForce(DiagramNode node, ForceType type, DrawingContext dc, Pen pen) {
            if (!node.Forces.ContainsKey(type)) {
                return;
            }
            var force = node.Forces[type];
            DrawForce(node, force, dc, pen);
        }

        private void DrawForce(DiagramNode node, Vector force, DrawingContext dc, Pen pen) {
            var pos0 = node.Pos;
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

        private bool isMouseLeftButtonDown;
        private Point startDraggingPoint;

        public void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var pos = e.GetPosition(this);
            startDraggingPoint = pos;
            e.Handled = diagram.UmlDiagramInteractor.HandleMouseLeftButtonDown(pos);
            isMouseLeftButtonDown = true;
            Mouse.Capture(this);
        }

        public void OnMouseMove(object sender, MouseEventArgs e) {
            if (isMouseLeftButtonDown) {
                var pos = e.GetPosition(this);
                e.Handled = diagram.UmlDiagramInteractor.HandleMouseMove(pos, startDraggingPoint);
            }
        }

        public void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var pos = e.GetPosition(this);
            if (isMouseLeftButtonDown) {
                e.Handled = diagram.UmlDiagramInteractor.HandleMouseLeftButtonUp(pos);
                isMouseLeftButtonDown = false;
            }
            Mouse.Capture(null);
        }
    }
}
