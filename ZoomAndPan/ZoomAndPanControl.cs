using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ZoomAndPan {
    /// <summary>
    /// A class that wraps up zooming and panning of it's content.
    /// </summary>
    public partial class ZoomAndPanControl : ContentControl, IScrollInfo {
        #region Internal Data Members

        /// <summary>
        /// Reference to the underlying content, which is named PART_Content in the template.
        /// </summary>
        private FrameworkElement content;

        /// <summary>
        /// The transform that is applied to the content to scale it by 'ContentScale'.
        /// </summary>
        private ScaleTransform contentScaleTransform;

        /// <summary>
        /// The transform that is applied to the content to offset it by 'ViewportOffsetXInCC' and 'ViewportOffsetYInCC'.
        /// </summary>
        private TranslateTransform contentOffsetTransformInCC;

        /// <summary>
        /// Enable the update of the content offset as the content scale changes.
        /// This enabled for zooming about a point (google-maps style zooming) and zooming to a rect.
        /// </summary>
        private bool enableContentOffsetUpdateFromScale;

        /// <summary>
        /// Used to disable syncronization between IScrollInfo interface and ViewportOffsetXInCC/ViewportOffsetYInCC.
        /// </summary>
        private bool disableScrollOffsetSync;

        /// <summary>
        /// Normally when content offsets changes the content focus is automatically updated.
        /// This syncronization is disabled when 'disableContentFocusSync' is set to 'true'.
        /// When we are zooming in or out we 'disableContentFocusSync' is set to 'true' because 
        /// we are zooming in or out relative to the content focus we don't want to update the focus.
        /// </summary>
        private bool disableContentFocusSync;

        /// <summary>
        /// The width of the viewport in content coordinates, clamped to the width of the content.
        /// </summary>
        private double constrainedViewportWidthInCC;

        /// <summary>
        /// The height of the viewport in content coordinates, clamped to the height of the content.
        /// </summary>
        private double constrainedViewportHeightInCC;

        #endregion Internal Data Members

        #region IScrollInfo Data Members

        //
        // These data members are for the implementation of the IScrollInfo interface.
        // This interface works with the ScrollViewer such that when ZoomAndPanControl is 
        // wrapped (in XAML) with a ScrollViewer the IScrollInfo interface allows the ZoomAndPanControl to
        // handle the the scrollbar offsets.
        //
        // The IScrollInfo properties and member functions are implemented in ZoomAndPanControl_IScrollInfo.cs.
        //
        // There is a good series of articles showing how to implement IScrollInfo starting here:
        //     http://blogs.msdn.com/bencon/archive/2006/01/05/509991.aspx
        //

        /// <summary>
        /// Set to 'true' when the vertical scrollbar is enabled.
        /// </summary>
        private bool canVerticallyScroll;

        /// <summary>
        /// Set to 'true' when the vertical scrollbar is enabled.
        /// </summary>
        private bool canHorizontallyScroll;

        /// <summary>
        /// Records the unscaled extent of the content.
        /// This is calculated during the measure and arrange.
        /// </summary>
        private Size unScaledExtent = new Size(0, 0);

        /// <summary>
        /// Records the size of the viewport (in viewport coordinates) onto the content.
        /// This is calculated during the measure and arrange.
        /// </summary>
        private Size viewportSize = new Size(0, 0);

        /// <summary>
        /// Reference to the ScrollViewer that is wrapped (in XAML) around the ZoomAndPanControl.
        /// Or set to null if there is no ScrollViewer.
        /// </summary>
        private ScrollViewer scrollOwner;

        #endregion IScrollInfo Data Members

        #region Dependency Property Definitions

        //
        // Definitions for dependency properties.
        //

        public static readonly DependencyProperty ContentScaleProperty =
                DependencyProperty.Register("ContentScale", typeof(double), typeof(ZoomAndPanControl),
                                            new FrameworkPropertyMetadata(1.0, ContentScalePropertyChanged, ContentScaleCoerce));

        public static readonly DependencyProperty MinContentScaleProperty =
                DependencyProperty.Register("MinContentScale", typeof(double), typeof(ZoomAndPanControl),
                                            new FrameworkPropertyMetadata(0.01, MinOrMaxContentScalePropertyChanged));

        public static readonly DependencyProperty MaxContentScaleProperty =
                DependencyProperty.Register("MaxContentScale", typeof(double), typeof(ZoomAndPanControl),
                                            new FrameworkPropertyMetadata(10.0, MinOrMaxContentScalePropertyChanged));

        public static readonly DependencyProperty ViewportOffsetXInCCProperty =
                DependencyProperty.Register("ViewportOffsetXInCC", typeof(double), typeof(ZoomAndPanControl),
                                            new FrameworkPropertyMetadata(0.0, ViewportOffsetXInCCPropertyChanged, ViewportOffsetXInCCCoerce));

        public static readonly DependencyProperty ViewportOffsetYInCCProperty =
                DependencyProperty.Register("ViewportOffsetYInCC", typeof(double), typeof(ZoomAndPanControl),
                                            new FrameworkPropertyMetadata(0.0, ViewportOffsetYInCCPropertyChanged, ViewportOffsetYInCCCoerce));

        public static readonly DependencyProperty AnimationDurationProperty =
                DependencyProperty.Register("AnimationDuration", typeof(double), typeof(ZoomAndPanControl),
                                            new FrameworkPropertyMetadata(0.4));

        public static readonly DependencyProperty ContentZoomFocusXProperty =
                DependencyProperty.Register("ContentZoomFocusX", typeof(double), typeof(ZoomAndPanControl),
                                            new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty ContentZoomFocusYProperty =
                DependencyProperty.Register("ContentZoomFocusY", typeof(double), typeof(ZoomAndPanControl),
                                            new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty ViewportZoomFocusXProperty =
                DependencyProperty.Register("ViewportZoomFocusX", typeof(double), typeof(ZoomAndPanControl),
                                            new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty ViewportZoomFocusYProperty =
                DependencyProperty.Register("ViewportZoomFocusY", typeof(double), typeof(ZoomAndPanControl),
                                            new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty ViewportWidthInCCProperty =
                DependencyProperty.Register("ViewportWidthInCC", typeof(double), typeof(ZoomAndPanControl),
                                            new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty ViewportHeightInCCProperty =
                DependencyProperty.Register("ViewportHeightInCC", typeof(double), typeof(ZoomAndPanControl),
                                            new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty IsMouseWheelScrollingEnabledProperty =
                DependencyProperty.Register("IsMouseWheelScrollingEnabled", typeof(bool), typeof(ZoomAndPanControl),
                                            new FrameworkPropertyMetadata(false));

        #endregion Dependency Property Definitions

        /// <summary>
        /// Get/set the X offset (in content coordinates) of the view on the content.
        /// </summary>
        public double ViewportOffsetXInCC {
            get {
                return (double)GetValue(ViewportOffsetXInCCProperty);
            }
            set {
                SetValue(ViewportOffsetXInCCProperty, value);
            }
        }

        /// <summary>
        /// Event raised when the ViewportOffsetXInCC property has changed.
        /// </summary>
        public event EventHandler ViewportOffsetXInCCChanged;

        /// <summary>
        /// Get/set the Y offset (in content coordinates) of the view on the content.
        /// </summary>
        public double ViewportOffsetYInCC {
            get {
                return (double)GetValue(ViewportOffsetYInCCProperty);
            }
            set {
                SetValue(ViewportOffsetYInCCProperty, value);
            }
        }

        /// <summary>
        /// Event raised when the ViewportOffsetYInCC property has changed.
        /// </summary>
        public event EventHandler ViewportOffsetYInCCChanged;

        /// <summary>
        /// Get/set the current scale (or zoom factor) of the content.
        /// </summary>
        public double ContentScale {
            get {
                return (double)GetValue(ContentScaleProperty);
            }
            set {
                SetValue(ContentScaleProperty, value);
            }
        }

        /// <summary>
        /// Event raised when the ContentScale property has changed.
        /// </summary>
        public event EventHandler ContentScaleChanged;

        /// <summary>
        /// Get/set the minimum value for 'ContentScale'.
        /// </summary>
        public double MinContentScale {
            get {
                return (double)GetValue(MinContentScaleProperty);
            }
            set {
                SetValue(MinContentScaleProperty, value);
            }
        }

        /// <summary>
        /// Get/set the maximum value for 'ContentScale'.
        /// </summary>
        public double MaxContentScale {
            get {
                return (double)GetValue(MaxContentScaleProperty);
            }
            set {
                SetValue(MaxContentScaleProperty, value);
            }
        }

        /// <summary>
        /// The X coordinate of the content focus, this is the point that we are focusing on when zooming.
        /// </summary>
        public double ContentZoomFocusX {
            get {
                return (double)GetValue(ContentZoomFocusXProperty);
            }
            set {
                SetValue(ContentZoomFocusXProperty, value);
            }
        }

        /// <summary>
        /// The Y coordinate of the content focus, this is the point that we are focusing on when zooming.
        /// </summary>
        public double ContentZoomFocusY {
            get {
                return (double)GetValue(ContentZoomFocusYProperty);
            }
            set {
                SetValue(ContentZoomFocusYProperty, value);
            }
        }

        /// <summary>
        /// The X coordinate of the viewport focus, this is the point in the viewport (in viewport coordinates) 
        /// that the content focus point is locked to while zooming in.
        /// </summary>
        public double ZoomFocusXInVC {
            get {
                return (double)GetValue(ViewportZoomFocusXProperty);
            }
            set {
                SetValue(ViewportZoomFocusXProperty, value);
            }
        }

        /// <summary>
        /// The Y coordinate of the viewport focus, this is the point in the viewport (in viewport coordinates) 
        /// that the content focus point is locked to while zooming in.
        /// </summary>
        public double ZoomFocusYInVC {
            get {
                return (double)GetValue(ViewportZoomFocusYProperty);
            }
            set {
                SetValue(ViewportZoomFocusYProperty, value);
            }
        }

        /// <summary>
        /// Get the viewport width, in content coordinates.
        /// </summary>
        public double ViewportWidthInCC {
            get {
                return (double)GetValue(ViewportWidthInCCProperty);
            }
            set {
                SetValue(ViewportWidthInCCProperty, value);
            }
        }

        /// <summary>
        /// Get the viewport height, in content coordinates.
        /// </summary>
        public double ViewportHeightInCC {
            get {
                return (double)GetValue(ViewportHeightInCCProperty);
            }
            set {
                SetValue(ViewportHeightInCCProperty, value);
            }
        }

        /// <summary>
        /// The duration of the animations (in seconds) started by calling AnimatedZoomTo and the other animation methods.
        /// </summary>
        public double AnimationDuration {
            get {
                return (double)GetValue(AnimationDurationProperty);
            }
            set {
                SetValue(AnimationDurationProperty, value);
            }
        }

        /// <summary>
        /// Set to 'true' to enable the mouse wheel to scroll the zoom and pan control.
        /// This is set to 'false' by default.
        /// </summary>
        public bool IsMouseWheelScrollingEnabled {
            get {
                return (bool)GetValue(IsMouseWheelScrollingEnabledProperty);
            }
            set {
                SetValue(IsMouseWheelScrollingEnabledProperty, value);
            }
        }

        /// <summary>
        /// Do an animated zoom to view a specific scale and rectangle (in content coordinates).
        /// </summary>
        public void AnimatedZoomToContentRect(double newScale, Rect contentRect) {
            AnimatedZoomPointToViewportCenter(newScale, new Point(contentRect.X + (contentRect.Width / 2), contentRect.Y + (contentRect.Height / 2)),
                delegate {
                    //
                    // At the end of the animation, ensure that we are snapped to the specified content offset.
                    // Due to zooming in on the content focus point and rounding errors, the content offset may
                    // be slightly off what we want at the end of the animation and this bit of code corrects it.
                    //
                    ViewportOffsetXInCC = contentRect.X;
                    ViewportOffsetYInCC = contentRect.Y;
                });
        }

        /// <summary>
        /// Do an animated zoom to the specified rectangle (in content coordinates).
        /// </summary>
        public void AnimatedZoomToContentRect(Rect contentRect) {
            double scaleX = ViewportWidthInCC / contentRect.Width;
            double scaleY = ViewportHeightInCC / contentRect.Height;
            double newScale = ContentScale * Math.Min(scaleX, scaleY);

            AnimatedZoomPointToViewportCenter(newScale, new Point(contentRect.X + (contentRect.Width / 2), contentRect.Y + (contentRect.Height / 2)), null);
        }

        /// <summary>
        /// Instantly zoom to the specified rectangle (in content coordinates).
        /// </summary>
        public void ZoomToContentRect(Rect contentRect) {
            double scaleX = ViewportWidthInCC / contentRect.Width;
            double scaleY = ViewportHeightInCC / contentRect.Height;
            double newScale = ContentScale * Math.Min(scaleX, scaleY);

            ZoomPointToViewportCenter(newScale, new Point(contentRect.X + (contentRect.Width / 2), contentRect.Y + (contentRect.Height / 2)));
        }

        /// <summary>
        /// Instantly center the view on the specified point (in content coordinates).
        /// </summary>
        public void SnapContentOffsetTo(Point contentOffset) {
            AnimationHelper.CancelAnimation(this, ViewportOffsetXInCCProperty);
            AnimationHelper.CancelAnimation(this, ViewportOffsetYInCCProperty);

            ViewportOffsetXInCC = contentOffset.X;
            ViewportOffsetYInCC = contentOffset.Y;
        }

        /// <summary>
        /// Instantly center the view on the specified point (in content coordinates).
        /// </summary>
        public void SnapTo(Point contentPoint) {
            AnimationHelper.CancelAnimation(this, ViewportOffsetXInCCProperty);
            AnimationHelper.CancelAnimation(this, ViewportOffsetYInCCProperty);

            ViewportOffsetXInCC = contentPoint.X - (ViewportWidthInCC / 2);
            ViewportOffsetYInCC = contentPoint.Y - (ViewportHeightInCC / 2);
        }

        /// <summary>
        /// Use animation to center the view on the specified point (in content coordinates).
        /// </summary>
        public void AnimatedSnapTo(Point contentPoint) {
            double newX = contentPoint.X - (ViewportWidthInCC / 2);
            double newY = contentPoint.Y - (ViewportHeightInCC / 2);

            AnimationHelper.StartAnimation(this, ViewportOffsetXInCCProperty, newX, AnimationDuration);
            AnimationHelper.StartAnimation(this, ViewportOffsetYInCCProperty, newY, AnimationDuration);
        }

        /// <summary>
        /// Zoom in/out centered on the specified point (in content coordinates).
        /// The focus point is kept locked to it's on screen position (ala google maps).
        /// </summary>
        public void AnimatedZoomAboutPoint(double newContentScale, Point contentZoomFocus) {
            newContentScale = Math.Min(Math.Max(newContentScale, MinContentScale), MaxContentScale);

            AnimationHelper.CancelAnimation(this, ContentZoomFocusXProperty);
            AnimationHelper.CancelAnimation(this, ContentZoomFocusYProperty);
            AnimationHelper.CancelAnimation(this, ViewportZoomFocusXProperty);
            AnimationHelper.CancelAnimation(this, ViewportZoomFocusYProperty);

            ContentZoomFocusX = contentZoomFocus.X;
            ContentZoomFocusY = contentZoomFocus.Y;
            ZoomFocusXInVC = (ContentZoomFocusX - ViewportOffsetXInCC) * ContentScale;
            ZoomFocusYInVC = (ContentZoomFocusY - ViewportOffsetYInCC) * ContentScale;

            //
            // When zooming about a point make updates to ContentScale also update content offset.
            //
            enableContentOffsetUpdateFromScale = true;

            AnimationHelper.StartAnimation(this, ContentScaleProperty, newContentScale, AnimationDuration,
                delegate {
                    enableContentOffsetUpdateFromScale = false;

                    ResetViewportZoomFocus();
                });
        }

        /// <summary>
        /// Zoom in/out centered on the specified point (in content coordinates).
        /// The focus point is kept locked to it's on screen position (ala google maps).
        /// </summary>
        public void ZoomAboutPoint(double newContentScale, Point contentZoomFocus) {
            newContentScale = Math.Min(Math.Max(newContentScale, MinContentScale), MaxContentScale);

            double screenSpaceZoomOffsetX = (contentZoomFocus.X - ViewportOffsetXInCC) * ContentScale;
            double screenSpaceZoomOffsetY = (contentZoomFocus.Y - ViewportOffsetYInCC) * ContentScale;
            double contentSpaceZoomOffsetX = screenSpaceZoomOffsetX / newContentScale;
            double contentSpaceZoomOffsetY = screenSpaceZoomOffsetY / newContentScale;
            double newViewportOffsetXInCC = contentZoomFocus.X - contentSpaceZoomOffsetX;
            double newViewportOffsetYInCC = contentZoomFocus.Y - contentSpaceZoomOffsetY;

            AnimationHelper.CancelAnimation(this, ContentScaleProperty);
            AnimationHelper.CancelAnimation(this, ViewportOffsetXInCCProperty);
            AnimationHelper.CancelAnimation(this, ViewportOffsetYInCCProperty);

            ContentScale = newContentScale;
            ViewportOffsetXInCC = newViewportOffsetXInCC;
            ViewportOffsetYInCC = newViewportOffsetYInCC;
        }

        /// <summary>
        /// Zoom in/out centered on the viewport center.
        /// </summary>
        public void AnimatedZoomToContentRect(double contentScale) {
            Point zoomCenter = new Point(ViewportOffsetXInCC + (ViewportWidthInCC / 2), ViewportOffsetYInCC + (ViewportHeightInCC / 2));
            AnimatedZoomAboutPoint(contentScale, zoomCenter);
        }

        /// <summary>
        /// Zoom in/out centered on the viewport center.
        /// </summary>
        public void ZoomToContentRect(double contentScale) {
            Point zoomCenter = new Point(ViewportOffsetXInCC + (ViewportWidthInCC / 2), ViewportOffsetYInCC + (ViewportHeightInCC / 2));
            ZoomAboutPoint(contentScale, zoomCenter);
        }

        /// <summary>
        /// Do animation that scales the content so that it fits completely in the control.
        /// </summary>
        public void AnimatedScaleToFit() {
            if (content == null) {
                throw new ApplicationException("PART_Content was not found in the ZoomAndPanControl visual template!");
            }

            AnimatedZoomToContentRect(new Rect(0, 0, content.ActualWidth, content.ActualHeight));
        }

        /// <summary>
        /// Instantly scale the content so that it fits completely in the control.
        /// </summary>
        public void ScaleToFit() {
            if (content == null) {
                throw new ApplicationException("PART_Content was not found in the ZoomAndPanControl visual template!");
            }

            ZoomToContentRect(new Rect(0, 0, content.ActualWidth, content.ActualHeight));
        }

        #region Internal Methods

        Canvas dragZoomCanvas;
        Border dragZoomBorder;

        /// <summary>
        /// Static constructor to define metadata for the control (and link it to the style in Generic.xaml).
        /// </summary>
        static ZoomAndPanControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomAndPanControl), new FrameworkPropertyMetadata(typeof(ZoomAndPanControl)));
        }

        public ZoomAndPanControl() {
            MouseDown += ZoomAndPanControlMouseDown;
            MouseUp += ZoomAndPanControlMouseUp;
            MouseMove += ZoomAndPanControlMouseMove;
            MouseWheel += ZoomAndPanControlMouseWheel;
            MouseDoubleClick += ZoomAndPanControlMouseDoubleClick;
        }

        /// <summary>
        /// Called when a template has been applied to the control.
        /// </summary>
        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            content = Template.FindName("PART_Content", this) as FrameworkElement;
            if (content != null) {
                //
                // Setup the transform on the content so that we can scale it by 'ContentScale'.
                //
                contentScaleTransform = new ScaleTransform(ContentScale, ContentScale);

                //
                // Setup the transform on the content so that we can translate it by 'ViewportOffsetXInCC' and 'ViewportOffsetYInCC'.
                //
                contentOffsetTransformInCC = new TranslateTransform();
                UpdateTranslationX();
                UpdateTranslationY();

                //
                // Setup a transform group to contain the translation and scale transforms, and then
                // assign this to the content's 'RenderTransform'.
                //
                TransformGroup transformGroup = new TransformGroup();
                transformGroup.Children.Add(contentOffsetTransformInCC);
                transformGroup.Children.Add(contentScaleTransform);
                content.RenderTransform = transformGroup;

                dragZoomCanvas = Template.FindName("dragZoomCanvas", this) as Canvas;
                dragZoomBorder = Template.FindName("dragZoomBorder", this) as Border;
            }
        }

        /// <summary>
        /// Zoom to the specified scale and move the specified focus point to the center of the viewport.
        /// </summary>
        private void AnimatedZoomPointToViewportCenter(double newContentScale, Point contentZoomFocus, EventHandler callback) {
            newContentScale = Math.Min(Math.Max(newContentScale, MinContentScale), MaxContentScale);

            AnimationHelper.CancelAnimation(this, ContentZoomFocusXProperty);
            AnimationHelper.CancelAnimation(this, ContentZoomFocusYProperty);
            AnimationHelper.CancelAnimation(this, ViewportZoomFocusXProperty);
            AnimationHelper.CancelAnimation(this, ViewportZoomFocusYProperty);

            ContentZoomFocusX = contentZoomFocus.X;
            ContentZoomFocusY = contentZoomFocus.Y;
            ZoomFocusXInVC = (ContentZoomFocusX - ViewportOffsetXInCC) * ContentScale;
            ZoomFocusYInVC = (ContentZoomFocusY - ViewportOffsetYInCC) * ContentScale;

            //
            // When zooming about a point make updates to ContentScale also update content offset.
            //
            enableContentOffsetUpdateFromScale = true;

            AnimationHelper.StartAnimation(this, ContentScaleProperty, newContentScale, AnimationDuration,
                delegate {
                    enableContentOffsetUpdateFromScale = false;

                    if (callback != null) {
                        callback(this, EventArgs.Empty);
                    }
                });

            AnimationHelper.StartAnimation(this, ViewportZoomFocusXProperty, ViewportWidth / 2, AnimationDuration);
            AnimationHelper.StartAnimation(this, ViewportZoomFocusYProperty, ViewportHeight / 2, AnimationDuration);
        }

        /// <summary>
        /// Zoom to the specified scale and move the specified focus point to the center of the viewport.
        /// </summary>
        private void ZoomPointToViewportCenter(double newContentScale, Point contentZoomFocus) {
            newContentScale = Math.Min(Math.Max(newContentScale, MinContentScale), MaxContentScale);

            AnimationHelper.CancelAnimation(this, ContentScaleProperty);
            AnimationHelper.CancelAnimation(this, ViewportOffsetXInCCProperty);
            AnimationHelper.CancelAnimation(this, ViewportOffsetYInCCProperty);

            ContentScale = newContentScale;
            ViewportOffsetXInCC = contentZoomFocus.X - (ViewportWidthInCC / 2);
            ViewportOffsetYInCC = contentZoomFocus.Y - (ViewportHeightInCC / 2);
        }

        /// <summary>
        /// Event raised when the 'ContentScale' property has changed value.
        /// </summary>
        private static void ContentScalePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
            ZoomAndPanControl c = (ZoomAndPanControl)o;

            if (c.contentScaleTransform != null) {
                //
                // Update the content scale transform whenever 'ContentScale' changes.
                //
                c.contentScaleTransform.ScaleX = c.ContentScale;
                c.contentScaleTransform.ScaleY = c.ContentScale;
            }

            //
            // Update the size of the viewport in content coordinates.
            //
            c.UpdateContentViewportSize();

            if (c.enableContentOffsetUpdateFromScale) {
                try {
                    // 
                    // Disable content focus syncronization.  We are about to update content offset whilst zooming
                    // to ensure that the viewport is focused on our desired content focus point.  Setting this
                    // to 'true' stops the automatic update of the content focus when content offset changes.
                    //
                    c.disableContentFocusSync = true;

                    //
                    // Whilst zooming in or out keep the content offset up-to-date so that the viewport is always
                    // focused on the content focus point (and also so that the content focus is locked to the 
                    // viewport focus point - this is how the google maps style zooming works).
                    //
                    double viewportOffsetX = c.ZoomFocusXInVC - (c.ViewportWidth / 2);
                    double viewportOffsetY = c.ZoomFocusYInVC - (c.ViewportHeight / 2);
                    double contentOffsetX = viewportOffsetX / c.ContentScale;
                    double contentOffsetY = viewportOffsetY / c.ContentScale;
                    c.ViewportOffsetXInCC = (c.ContentZoomFocusX - (c.ViewportWidthInCC / 2)) - contentOffsetX;
                    c.ViewportOffsetYInCC = (c.ContentZoomFocusY - (c.ViewportHeightInCC / 2)) - contentOffsetY;
                } finally {
                    c.disableContentFocusSync = false;
                }
            }

            if (c.ContentScaleChanged != null) {
                c.ContentScaleChanged(c, EventArgs.Empty);
            }

            if (c.scrollOwner != null) {
                c.scrollOwner.InvalidateScrollInfo();
            }
        }

        /// <summary>
        /// Method called to clamp the 'ContentScale' value to its valid range.
        /// </summary>
        private static object ContentScaleCoerce(DependencyObject d, object baseValue) {
            ZoomAndPanControl c = (ZoomAndPanControl)d;
            double value = (double)baseValue;
            value = Math.Min(Math.Max(value, c.MinContentScale), c.MaxContentScale);
            return value;
        }

        /// <summary>
        /// Event raised 'MinContentScale' or 'MaxContentScale' has changed.
        /// </summary>
        private static void MinOrMaxContentScalePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
            ZoomAndPanControl c = (ZoomAndPanControl)o;
            c.ContentScale = Math.Min(Math.Max(c.ContentScale, c.MinContentScale), c.MaxContentScale);
        }

        /// <summary>
        /// Event raised when the 'ViewportOffsetXInCC' property has changed value.
        /// </summary>
        private static void ViewportOffsetXInCCPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
            ZoomAndPanControl c = (ZoomAndPanControl)o;

            c.UpdateTranslationX();

            if (!c.disableContentFocusSync) {
                //
                // Normally want to automatically update content focus when content offset changes.
                // Although this is disabled using 'disableContentFocusSync' when content offset changes due to in-progress zooming.
                //
                c.UpdateContentZoomFocusX();
            }

            if (c.ViewportOffsetXInCCChanged != null) {
                //
                // Raise an event to let users of the control know that the content offset has changed.
                //
                c.ViewportOffsetXInCCChanged(c, EventArgs.Empty);
            }

            if (!c.disableScrollOffsetSync && c.scrollOwner != null) {
                //
                // Notify the owning ScrollViewer that the scrollbar offsets should be updated.
                //
                c.scrollOwner.InvalidateScrollInfo();
            }
        }

        /// <summary>
        /// Method called to clamp the 'ViewportOffsetXInCC' value to its valid range.
        /// </summary>
        private static object ViewportOffsetXInCCCoerce(DependencyObject d, object baseValue) {
            ZoomAndPanControl c = (ZoomAndPanControl)d;
            double value = (double)baseValue;
            double minOffsetX = 0.0;
            double maxOffsetX = Math.Max(0.0, c.unScaledExtent.Width - c.constrainedViewportWidthInCC);
            value = Math.Min(Math.Max(value, minOffsetX), maxOffsetX);
            return value;
        }

        /// <summary>
        /// Event raised when the 'ViewportOffsetYInCC' property has changed value.
        /// </summary>
        private static void ViewportOffsetYInCCPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
            ZoomAndPanControl c = (ZoomAndPanControl)o;

            c.UpdateTranslationY();

            if (!c.disableContentFocusSync) {
                //
                // Normally want to automatically update content focus when content offset changes.
                // Although this is disabled using 'disableContentFocusSync' when content offset changes due to in-progress zooming.
                //
                c.UpdateContentZoomFocusY();
            }

            if (c.ViewportOffsetYInCCChanged != null) {
                //
                // Raise an event to let users of the control know that the content offset has changed.
                //
                c.ViewportOffsetYInCCChanged(c, EventArgs.Empty);
            }

            if (!c.disableScrollOffsetSync && c.scrollOwner != null) {
                //
                // Notify the owning ScrollViewer that the scrollbar offsets should be updated.
                //
                c.scrollOwner.InvalidateScrollInfo();
            }

        }

        /// <summary>
        /// Method called to clamp the 'ViewportOffsetYInCC' value to its valid range.
        /// </summary>
        private static object ViewportOffsetYInCCCoerce(DependencyObject d, object baseValue) {
            ZoomAndPanControl c = (ZoomAndPanControl)d;
            double value = (double)baseValue;
            const double minOffsetY = 0.0;
            double maxOffsetY = Math.Max(0.0, c.unScaledExtent.Height - c.constrainedViewportHeightInCC);
            value = Math.Min(Math.Max(value, minOffsetY), maxOffsetY);
            return value;
        }

        /// <summary>
        /// Reset the viewport zoom focus to the center of the viewport.
        /// </summary>
        private void ResetViewportZoomFocus() {
            ZoomFocusXInVC = ViewportWidth / 2;
            ZoomFocusYInVC = ViewportHeight / 2;
        }

        /// <summary>
        /// Update the viewport size from the specified size.
        /// </summary>
        private void UpdateViewportSize(Size newSize) {
            if (viewportSize == newSize) {
                //
                // The viewport is already the specified size.
                //
                return;
            }

            viewportSize = newSize;

            //
            // Update the viewport size in content coordiates.
            //
            UpdateContentViewportSize();

            //
            // Initialise the content zoom focus point.
            //
            UpdateContentZoomFocusX();
            UpdateContentZoomFocusY();

            //
            // Reset the viewport zoom focus to the center of the viewport.
            //
            ResetViewportZoomFocus();

            //
            // Update content offset from itself when the size of the viewport changes.
            // This ensures that the content offset remains properly clamped to its valid range.
            //
            ViewportOffsetXInCC = ViewportOffsetXInCC;
            ViewportOffsetYInCC = ViewportOffsetYInCC;

            if (scrollOwner != null) {
                //
                // Tell that owning ScrollViewer that scrollbar data has changed.
                //
                scrollOwner.InvalidateScrollInfo();
            }
        }

        /// <summary>
        /// Update the size of the viewport in content coordinates after the viewport size or 'ContentScale' has changed.
        /// </summary>
        private void UpdateContentViewportSize() {
            ViewportWidthInCC = ViewportWidth / ContentScale;
            ViewportHeightInCC = ViewportHeight / ContentScale;

            constrainedViewportWidthInCC = Math.Min(ViewportWidthInCC, unScaledExtent.Width);
            constrainedViewportHeightInCC = Math.Min(ViewportHeightInCC, unScaledExtent.Height);

            UpdateTranslationX();
            UpdateTranslationY();
        }

        /// <summary>
        /// Update the X coordinate of the translation transformation.
        /// </summary>
        private void UpdateTranslationX() {
            if (contentOffsetTransformInCC != null) {
                double contentWidthInVC = unScaledExtent.Width * ContentScale;
                if (contentWidthInVC < ViewportWidth) {
                    //
                    // When the content can fit entirely within the viewport, center it.
                    //
                    contentOffsetTransformInCC.X = (ViewportWidthInCC - unScaledExtent.Width) / 2;
                } else {
                    contentOffsetTransformInCC.X = -ViewportOffsetXInCC;
                }
            }
        }

        /// <summary>
        /// Update the Y coordinate of the translation transformation.
        /// </summary>
        private void UpdateTranslationY() {
            if (contentOffsetTransformInCC != null) {
                double scaledContentHeight = unScaledExtent.Height * ContentScale;
                if (scaledContentHeight < ViewportHeight) {
                    //
                    // When the content can fit entirely within the viewport, center it.
                    //
                    contentOffsetTransformInCC.Y = (ViewportHeightInCC - unScaledExtent.Height) / 2;
                } else {
                    contentOffsetTransformInCC.Y = -ViewportOffsetYInCC;
                }
            }
        }

        /// <summary>
        /// Update the X coordinate of the zoom focus point in content coordinates.
        /// </summary>
        private void UpdateContentZoomFocusX() {
            ContentZoomFocusX = ViewportOffsetXInCC + (constrainedViewportWidthInCC / 2);
        }

        /// <summary>
        /// Update the Y coordinate of the zoom focus point in content coordinates.
        /// </summary>
        private void UpdateContentZoomFocusY() {
            ContentZoomFocusY = ViewportOffsetYInCC + (constrainedViewportHeightInCC / 2);
        }

        /// <summary>
        /// Measure the control and it's children.
        /// </summary>
        protected override Size MeasureOverride(Size constraint) {
            Size infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            Size childSize = base.MeasureOverride(infiniteSize);

            if (childSize != unScaledExtent) {
                //
                // Use the size of the child as the un-scaled extent content.
                //
                unScaledExtent = childSize;

                if (scrollOwner != null) {
                    scrollOwner.InvalidateScrollInfo();
                }
            }

            //
            // Update the size of the viewport onto the content based on the passed-in 'constraint'.
            //
            UpdateViewportSize(constraint);

            double width = constraint.Width;
            double height = constraint.Height;

            if (double.IsInfinity(width)) {
                //
                // Make sure we don't return infinity!
                //
                width = childSize.Width;
            }

            if (double.IsInfinity(height)) {
                //
                // Make sure we don't return infinity!
                //
                height = childSize.Height;
            }

            UpdateTranslationX();
            UpdateTranslationY();

            return new Size(width, height);
        }

        /// <summary>
        /// Arrange the control and it's children.
        /// </summary>
        protected override Size ArrangeOverride(Size arrangeBounds) {
            Size size = base.ArrangeOverride(DesiredSize);

            if (content.DesiredSize != unScaledExtent) {
                //
                // Use the size of the child as the un-scaled extent content.
                //
                unScaledExtent = content.DesiredSize;

                if (scrollOwner != null) {
                    scrollOwner.InvalidateScrollInfo();
                }
            }

            //
            // Update the size of the viewport onto the content based on the passed in 'arrangeBounds'.
            //
            UpdateViewportSize(arrangeBounds);

            return size;
        }

        #endregion Internal Methods

        #region Mouse events

        /// <summary>
        /// Specifies the current state of the mouse handling logic.
        /// </summary>
        private MouseHandlingMode mouseHandlingMode = MouseHandlingMode.None;

        /// <summary>
        /// The point that was clicked relative to the ZoomAndPanControl.
        /// </summary>
        private Point origZoomAndPanControlMouseDownPoint;

        /// <summary>
        /// The point that was clicked relative to the content that is contained within the ZoomAndPanControl.
        /// </summary>
        private Point origContentMouseDownPoint;

        /// <summary>
        /// Records which mouse button clicked during mouse dragging.
        /// </summary>
        private MouseButton mouseButtonDown;

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


        /////// <summary>
        /////// Event raised when the Window has loaded.
        /////// </summary>
        ////private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        ////{
        ////    OverviewWindow overviewWindow = new OverviewWindow();
        ////    overviewWindow.Left = Left;
        ////    overviewWindow.Top = Top + Height + 5;
        ////    overviewWindow.Owner = this;
        ////    overviewWindow.Show();

        ////    HelpTextWindow helpTextWindow = new HelpTextWindow();
        ////    helpTextWindow.Left = Left + Width + 5;
        ////    helpTextWindow.Top = Top;
        ////    helpTextWindow.Owner = this;
        ////    helpTextWindow.Show();

        ////    ExpandContent();            
        ////}

        ///// <summary>
        ///// Expand the content area to fit the rectangles.
        ///// </summary>
        //private void ExpandContent()
        //{
        //    double xOffset = 0;
        //    double yOffset = 0;
        //    Rect contentRect = new Rect(0, 0, 0, 0);
        //    foreach (RectangleData rectangleData in DataModel.Instance.Rectangles)
        //    {
        //        if (rectangleData.X < xOffset)
        //        {
        //            xOffset = rectangleData.X;
        //        }

        //        if (rectangleData.Y < yOffset)
        //        {
        //            yOffset = rectangleData.Y;
        //        }

        //        contentRect.Union(new Rect(rectangleData.X, rectangleData.Y, rectangleData.Width, rectangleData.Height));
        //    }

        //    //
        //    // Translate all rectangles so they are in positive space.
        //    //
        //    xOffset = Math.Abs(xOffset);
        //    yOffset = Math.Abs(yOffset);

        //    foreach (RectangleData rectangleData in DataModel.Instance.Rectangles)
        //    {
        //        rectangleData.X += xOffset;
        //        rectangleData.Y += yOffset;
        //    }

        //    DataModel.Instance.ContentWidth = contentRect.Width;
        //    DataModel.Instance.ContentHeight = contentRect.Height;
        //}

        /// <summary>
        /// Event raised on mouse down in the ZoomAndPanControl.
        /// </summary>
        private void ZoomAndPanControlMouseDown(object sender, MouseButtonEventArgs e) {
            content.Focus();
            Keyboard.Focus(content);

            mouseButtonDown = e.ChangedButton;
            origZoomAndPanControlMouseDownPoint = e.GetPosition(this);
            origContentMouseDownPoint = e.GetPosition(content);

            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 &&
                (e.ChangedButton == MouseButton.Left ||
                 e.ChangedButton == MouseButton.Right)) {
                // Shift + (left- or right-down) initiates zooming mode.
                mouseHandlingMode = MouseHandlingMode.Zooming;
            } else if (mouseButtonDown == MouseButton.Left) {
                // Just a plain old left-down initiates panning mode.
                mouseHandlingMode = MouseHandlingMode.Panning;
            }

            if (mouseHandlingMode != MouseHandlingMode.None) {
                // Capture the mouse so that we eventually receive the mouse up event.
                CaptureMouse();
                // RvD Don't handle the event, because in the overview window, we also want to handle this event
                //e.Handled = true;
            }
        }

        /// <summary>
        /// Event raised on mouse up in the ZoomAndPanControl.
        /// </summary>
        private void ZoomAndPanControlMouseUp(object sender, MouseButtonEventArgs e) {
            if (mouseHandlingMode != MouseHandlingMode.None) {
                switch (mouseHandlingMode) {
                    case MouseHandlingMode.Zooming:
                        switch (mouseButtonDown) {
                            case MouseButton.Left:
                                ZoomIn(origContentMouseDownPoint);
                                break;
                            case MouseButton.Right:
                                ZoomOut(origContentMouseDownPoint);
                                break;
                        }
                        break;
                    case MouseHandlingMode.DragZooming:
                        ApplyDragZoomRect();
                        break;
                }

                ReleaseMouseCapture();
                mouseHandlingMode = MouseHandlingMode.None;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Event raised on mouse move in the ZoomAndPanControl.
        /// </summary>
        private void ZoomAndPanControlMouseMove(object sender, MouseEventArgs e) {
            switch (mouseHandlingMode) {
                case MouseHandlingMode.Panning: {
                        //
                        // The user is left-dragging the mouse.
                        // Pan the viewport by the appropriate amount.
                        //
                        Point curContentMousePoint = e.GetPosition(content);
                        Vector dragOffset = curContentMousePoint - origContentMouseDownPoint;
                    Debug.WriteLine("dragoffset: " + dragOffset.ToString());
                        ViewportOffsetXInCC -= dragOffset.X;
                        ViewportOffsetYInCC -= dragOffset.Y;

                        e.Handled = true;
                    }
                    break;
                case MouseHandlingMode.Zooming: {
                        Point curZoomAndPanControlMousePoint = e.GetPosition(this);
                        Vector dragOffset = curZoomAndPanControlMousePoint - origZoomAndPanControlMouseDownPoint;
                        const double dragThreshold = 10;
                        if (mouseButtonDown == MouseButton.Left &&
                            (Math.Abs(dragOffset.X) > dragThreshold ||
                             Math.Abs(dragOffset.Y) > dragThreshold)) {
                            //
                            // When Shift + left-down zooming mode and the user drags beyond the drag threshold,
                            // initiate drag zooming mode where the user can drag out a rectangle to select the area
                            // to zoom in on.
                            //
                            mouseHandlingMode = MouseHandlingMode.DragZooming;
                            Point curContentMousePoint = e.GetPosition(content);
                            InitDragZoomRect(origContentMouseDownPoint, curContentMousePoint);
                        }

                        e.Handled = true;
                    }
                    break;
                case MouseHandlingMode.DragZooming: {
                        //
                        // When in drag zooming mode continously update the position of the rectangle
                        // that the user is dragging out.
                        //
                        Point curContentMousePoint = e.GetPosition(content);
                        SetDragZoomRect(origContentMouseDownPoint, curContentMousePoint);

                        e.Handled = true;
                    }
                    break;
            }
        }

        /// <summary>
        /// Event raised by rotating the mouse wheel
        /// </summary>
        private void ZoomAndPanControlMouseWheel(object sender, MouseWheelEventArgs e) {
            e.Handled = true;

            if (e.Delta > 0) {
                Point curContentMousePoint = e.GetPosition(content);
                ZoomIn(curContentMousePoint);
            } else if (e.Delta < 0) {
                Point curContentMousePoint = e.GetPosition(content);
                ZoomOut(curContentMousePoint);
            }
        }

        /// <summary>
        /// Zoom the viewport out, centering on the specified point (in content coordinates).
        /// </summary>
        private void ZoomOut(Point contentZoomCenter) {
            ZoomAboutPoint(ContentScale - 0.1, contentZoomCenter);
        }

        /// <summary>
        /// Zoom the viewport in, centering on the specified point (in content coordinates).
        /// </summary>
        private void ZoomIn(Point contentZoomCenter) {
            ZoomAboutPoint(ContentScale + 0.1, contentZoomCenter);
        }

        /// <summary>
        /// Initialise the rectangle that the use is dragging out.
        /// </summary>
        private void InitDragZoomRect(Point pt1, Point pt2) {
            SetDragZoomRect(pt1, pt2);

            dragZoomCanvas.Visibility = Visibility.Visible;
            dragZoomBorder.Opacity = 0.5;
        }

        /// <summary>
        /// Update the position and size of the rectangle that user is dragging out.
        /// </summary>
        private void SetDragZoomRect(Point pt1, Point pt2) {
            double x, y, width, height;

            //
            // Deterine x,y,width and height of the rect inverting the points if necessary.
            // 

            if (pt2.X < pt1.X) {
                x = pt2.X;
                width = pt1.X - pt2.X;
            } else {
                x = pt1.X;
                width = pt2.X - pt1.X;
            }

            if (pt2.Y < pt1.Y) {
                y = pt2.Y;
                height = pt1.Y - pt2.Y;
            } else {
                y = pt1.Y;
                height = pt2.Y - pt1.Y;
            }

            //
            // Update the coordinates of the rectangle that is being dragged out by the user.
            // The we offset and rescale to convert from content coordinates.
            //
            Canvas.SetLeft(dragZoomBorder, x);
            Canvas.SetTop(dragZoomBorder, y);
            dragZoomBorder.Width = width;
            dragZoomBorder.Height = height;
        }

        /// <summary>
        /// When the user has finished dragging out the rectangle the zoom operation is applied.
        /// </summary>
        private void ApplyDragZoomRect() {
            //
            // Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
            //
            SavePrevZoomRect();

            //
            // Retreive the rectangle that the user draggged out and zoom in on it.
            //
            double contentX = Canvas.GetLeft(dragZoomBorder);
            double contentY = Canvas.GetTop(dragZoomBorder);
            double contentWidth = dragZoomBorder.Width;
            double contentHeight = dragZoomBorder.Height;
            AnimatedZoomToContentRect(new Rect(contentX, contentY, contentWidth, contentHeight));

            FadeOutDragZoomRect();
        }

        //
        // Fade out the drag zoom rectangle.
        //
        private void FadeOutDragZoomRect() {
            AnimationHelper.StartAnimation(dragZoomBorder, OpacityProperty, 0.0, 0.1,
                delegate {
                    dragZoomCanvas.Visibility = Visibility.Collapsed;
                });
        }

        //
        // Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
        //
        private void SavePrevZoomRect() {
            prevZoomRect = new Rect(ViewportOffsetXInCC, ViewportOffsetYInCC, ViewportWidthInCC, ViewportHeightInCC);
            prevZoomScale = ContentScale;
            prevZoomRectSet = true;
        }

        /// <summary>
        /// Clear the memory of the previous zoom level.
        /// </summary>
        private void ClearPrevZoomRect() {
            prevZoomRectSet = false;
        }

        /// <summary>
        /// Event raised when the user has double clicked in the zoom and pan control.
        /// </summary>
        private void ZoomAndPanControlMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0) {
                Point doubleClickPoint = e.GetPosition(content);
                AnimatedSnapTo(doubleClickPoint);
            }
        }

        #endregion
    }
}
