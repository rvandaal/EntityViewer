using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GraphFramework;
using ZoomAndPan;
using System.Reflection;

namespace UmlEditor {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        /// <summary>
        /// Specifies the current state of the mouse handling logic.
        /// </summary>
        private MouseHandlingMode mouseHandlingMode = MouseHandlingMode.None;

        /// <summary>
        /// The point that was clicked relative to the content that is contained within the ZoomAndPanControl.
        /// </summary>
        private Point origContentMouseDownPoint;

        /// <summary>
        /// Saves the previous zoom rectangle, pressing the backspace key jumps back to this zoom rectangle.
        /// </summary>
        private Rect prevZoomRect;

        /// <summary>
        /// Save the previous content scale, pressing the backspace key jumps back to this scale.
        /// </summary>
        private double prevZoomScale;

        /// <summary>
        /// Set to 'true' when the previous zoom rect is saved.
        /// </summary>
        private bool prevZoomRectSet;

        private MainWindowViewModel vm;

        public MainWindow(MainWindowViewModel viewModel) {
            DataContext = viewModel;
            vm = viewModel;
            InitializeComponent();
            if (Application.Current.Resources.MergedDictionaries.Count > 0) {
                Application.Current.Resources.MergedDictionaries[0] =
                    Application.Current.Resources.MergedDictionaries[0];
            }
            //CompositionTarget.Rendering += OnCompositionTargetRendering;
            CompositionTargetEx.FrameUpdating += OnCompositionTargetRendering;

            SubscribeForMouseEvents();
            Loaded += (s, e) => creatorAutoCompleteBox.Focus();

            //zoomAndPanControl.ScrollOwner = scrollViewer;
        }

        private void SubscribeForMouseEvents() {
            zoomAndPanControl.MouseLeftButtonDown += canvas.OnMouseLeftButtonDown;
            zoomAndPanControl.MouseLeftButtonUp += canvas.OnMouseLeftButtonUp;
            zoomAndPanControl.MouseRightButtonDown += canvas.OnMouseRightButtonDown;
            zoomAndPanControl.MouseRightButtonUp += canvas.OnMouseRightButtonUp;
            zoomAndPanControl.MouseMove += canvas.OnMouseMove;
        }

        private void OnCompositionTargetRendering(object sender, EventArgs e) {
            ExpandContent();
            CheckCurrentOperation();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
            //canvas.Width = mainGrid.ColumnDefinitions[1].ActualWidth;
            //canvas.Height = mainGrid.ActualHeight;
        }

        private ZoomAndPanViewModel ZoomAndPanViewModel {
            get { return ((MainWindowViewModel) DataContext).ZoomAndPanViewModel; }
        }

        private void CheckCurrentOperation() {
            currentOperationTextBlock.Text = vm.Diagram.CurrentOperation.ToString();
        }

        private bool first = true;

        /// <summary>
        /// Expand the content area to fit the rectangles.
        /// </summary>
        /// <remarks>
        /// Is called on CompositionTarget.Rendering.
        /// </remarks>
        private void ExpandContent() {

            // OK. UmlCanvas krijg de mouse events binnen voor de nodes. Als ik bv een node wil verplaatsen dan moet ik overal op het scherm mousevents kunnen ontvangen.
            // Dit betekent meteen al dat UmlCanvas altijd de hele viewport moet bedekken.
            // Tegelijkertijd moet UmlCanvas kleiner gemaakt kunnen worden om te zoomen. Als de scalefactor groter dan 1 wordt moet UmlCanvas dus zijn width en height aanpassen
            // en daarnaast mag de viewport nooit een negatieve offset hebben tov de canvas.
            // Dit wordt wel allemaal heel moeilijk. Wat logischer is, is dat ik de mouse events gewoon in de zoomandpancontrol ontvang en deze door delegeer naar het canvas.
            // Als je dit doet, hoeft het canvas ook niet meer te rescalen want hij staat children toe om buiten zijn grenzen te komen.



            //if (!first) return;
            //first = false;

            double minXOfNodeContainers = double.PositiveInfinity;
            double minYOfNodeContainers = double.PositiveInfinity;
            Rect contentRect = new Rect(0, 0, 0, 0);

            foreach(var child in canvas.Children) {
                var umlClassControl = child as UmlClassControl;
                if(umlClassControl != null) {
                    var umlClass = umlClassControl.UmlClass;
                    if (umlClass != null && umlClass.IsVisible) {
                        var x = Canvas.GetLeft(umlClassControl);
                        var y = Canvas.GetTop(umlClassControl);
                        var width = umlClassControl.ActualWidth;
                        var height = umlClassControl.ActualHeight;
                        if (x < minXOfNodeContainers) {
                            minXOfNodeContainers = x;
                        }
                        if (y < minYOfNodeContainers) {
                            minYOfNodeContainers = y;
                        }
                        // contentRect contains all node containers and is as small as possible
                        // UmlCanvas will get the same size as this contentRect because it binds to ZAPVM.ContentWidth and ZAPVM.ContentHeight
                        contentRect.Union(new Rect(x, y, width, height));
                        
                    }
                }
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
            if (vm.Diagram.CurrentOperation == DragOperation.None) {
                ZoomAndPanViewModel.ContentWidth = contentRect.Width;
                ZoomAndPanViewModel.ContentHeight = contentRect.Height;

                //if (!double.IsNaN(ZoomAndPanViewModel.ContentWidth) && ZoomAndPanViewModel.ContentWidth != 0 && ZoomAndPanViewModel.ViewportWidthInCC != 0) {
                //    ZoomAndPanViewModel.ViewportOffsetXInCC = contentRect.Width / 2;
                //    ZoomAndPanViewModel.ViewportOffsetYInCC = contentRect.Height / 2;
                ////    // ContentScale is applied to the ScaleTransform of the ContentPresenter of the ZAPC (and thus the UmlCanvas)
                ////    // ZAPVM.ViewPortWidthInCC is never set in this ZAPVM, but comes from the ZAPC, it is computed from the ViewPortWidthInVC and ContentScale
                //    ZoomAndPanViewModel.ContentScale = ZoomAndPanViewModel.ViewportWidthInCC /
                //                                       ZoomAndPanViewModel.ContentWidth;
                //    first = false;
                //}

                //
                // Translate all rectangles so they are in positive space.
                //
                minXOfNodeContainers = -minXOfNodeContainers;// Math.Abs(xOffset);
                minYOfNodeContainers = -minYOfNodeContainers;// Math.Abs(yOffset);
                //minXOfNodeContainers = Math.Abs(minXOfNodeContainers);
                //minYOfNodeContainers = Math.Abs(minYOfNodeContainers);
                var offsetVector = new Vector(minXOfNodeContainers, minYOfNodeContainers);

                //foreach (var child in canvas.Children) {
                //    var nodeContainer = child as NodeContainer;
                //    if (nodeContainer == null) continue;
                //    var umlClass = nodeContainer.UmlClass;
                //    if (umlClass == null) continue;
                //    if (!umlClass.IsVisible) continue;
                //    umlClass.Pos2D += offsetVector;
                //    nodeContainer.Update();
                //}
            }
        }

        /// <summary>
        /// The 'ZoomIn' command (bound to the plus key) was executed.
        /// </summary>
        void ZoomInExecuted(object sender, ExecutedRoutedEventArgs e) {
            ZoomIn(new Point(zoomAndPanControl.ContentZoomFocusX, zoomAndPanControl.ContentZoomFocusY));
        }

        /// <summary>
        /// The 'ZoomOut' command (bound to the minus key) was executed.
        /// </summary>
        private void ZoomOutExecuted(object sender, ExecutedRoutedEventArgs e) {
            ZoomOut(new Point(zoomAndPanControl.ContentZoomFocusX, zoomAndPanControl.ContentZoomFocusY));
        }

        /// <summary>
        /// The 'JumpBackToPrevZoom' command was executed.
        /// </summary>
        private void JumpBackToPrevZoomExecuted(object sender, ExecutedRoutedEventArgs e) {
            JumpBackToPrevZoom();
        }

        /// <summary>
        /// Determines whether the 'JumpBackToPrevZoom' command can be executed.
        /// </summary>
        private void JumpBackToPrevZoomCanExecuted(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = prevZoomRectSet;
        }

        /// <summary>
        /// The 'Fill' command was executed.
        /// </summary>
        private void FillExecuted(object sender, ExecutedRoutedEventArgs e) {
            SavePrevZoomRect();
            zoomAndPanControl.AnimatedScaleToFit();
        }

        /// <summary>
        /// The 'OneHundredPercent' command was executed.
        /// </summary>
        private void OneHundredPercentExecuted(object sender, ExecutedRoutedEventArgs e) {
            SavePrevZoomRect();
            zoomAndPanControl.AnimatedZoomToContentRect(1.0);
        }


        /// <summary>
        /// Jump back to the previous zoom level.
        /// </summary>
        private void JumpBackToPrevZoom() {
            zoomAndPanControl.AnimatedZoomToContentRect(prevZoomScale, prevZoomRect);
            ClearPrevZoomRect();
        }


        /// <summary>
        /// Zoom the viewport out, centering on the specified point (in content coordinates).
        /// </summary>
        private void ZoomOut(Point contentZoomCenter) {
            zoomAndPanControl.ZoomAboutPoint(zoomAndPanControl.ContentScale - 0.1, contentZoomCenter);
        }

        /// <summary>
        /// Zoom the viewport in, centering on the specified point (in content coordinates).
        /// </summary>
        private void ZoomIn(Point contentZoomCenter) {
            zoomAndPanControl.ZoomAboutPoint(zoomAndPanControl.ContentScale + 0.1, contentZoomCenter);
        }

        //
        // Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
        //
        private void SavePrevZoomRect() {
            prevZoomRect = new Rect(zoomAndPanControl.ViewportOffsetXInCC, zoomAndPanControl.ViewportOffsetYInCC, zoomAndPanControl.ViewportWidthInCC, zoomAndPanControl.ViewportHeightInCC);
            prevZoomScale = zoomAndPanControl.ContentScale;
            prevZoomRectSet = true;
        }

        /// <summary>
        /// Clear the memory of the previous zoom level.
        /// </summary>
        private void ClearPrevZoomRect() {
            prevZoomRectSet = false;
        }

        //private OnCreatorTextBoxPreviewKeyDown(object sender, KeyEventArgs e) {
            
        //}
    }
}
