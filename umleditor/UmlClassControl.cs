using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace UmlEditor {
    public class UmlClassControl : Control {

        #region Private field
        private TextBox classNameTextBox;
        private Border body;
        private ContentControl inheritanceConnector;
        private ContentControl compositionConnector;
        private ContentControl associationConnector;

        public UmlClass UmlClass { get; private set; }
        #endregion

        private readonly ObservableCollection<UmlAttribute> attributes = new ObservableCollection<UmlAttribute>();
        public ObservableCollection<UmlAttribute> Attributes {
            get { return attributes; }
        }

        private readonly ObservableCollection<UmlOperation> operations = new ObservableCollection<UmlOperation>();
        public ObservableCollection<UmlOperation> Operations {
            get { return operations; }
        }

        #region Constructors
        static UmlClassControl() {
            EventManager.RegisterClassHandler(
                typeof(UmlClassControl),
                MouseDoubleClickEvent,
                new RoutedEventHandler(OnMouseDoubleClick)
            );
        }

        public UmlClassControl(UmlClass umlClass) {
            UmlClass = umlClass;
            umlClass.PropertyChanged += OnPropertyChanged;
            umlClass.Attributes.CollectionChanged += OnAttributesChanged;
            umlClass.Operations.CollectionChanged += OnOperationsChanged;
            Name = umlClass.Name;
            MapNameToBackground();
            Update();
        }
        #endregion

        public void Update() {
            if (!double.IsNaN(UmlClass.TopLeft.X)) {
                Canvas.SetLeft(this, UmlClass.TopLeft2D.X);
                Canvas.SetTop(this, UmlClass.TopLeft2D.Y);
                MinWidth = UmlClass.Size2D.Width;
                MinHeight = UmlClass.Size2D.Height;
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);
            UmlClass.Size2D = RenderSize;
        }

        #region Background

        public Brush Background {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(UmlClassControl),
                new FrameworkPropertyMetadata(null)
            );

        #endregion

        #region StereoType

        public string StereoType {
            get { return (string)GetValue(StereoTypeProperty); }
            set { SetValue(StereoTypeProperty, value); }
        }

        public static readonly DependencyProperty StereoTypeProperty =
            DependencyProperty.Register("StereoType", typeof(string), typeof(UmlClassControl),
                new FrameworkPropertyMetadata(null)
            );

        #endregion



        #region MouseLeftButtonUpOnBody

        private void RaiseMouseLeftButtonUpOnBodyEvent() {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = MouseLeftButtonUpOnBodyEvent;
            RaiseEvent(args);
        }

        public event RoutedEventHandler MouseLeftButtonUpOnBody {
            add { AddHandler(MouseLeftButtonUpOnBodyEvent, value); }
            remove { RemoveHandler(MouseLeftButtonUpOnBodyEvent, value); }
        }

        public static readonly RoutedEvent MouseLeftButtonUpOnBodyEvent =
            EventManager.RegisterRoutedEvent(
                "MouseLeftButtonUpOnBody",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(UmlClassControl)
            );

        #endregion

        #region MouseLeftButtonUpOnInheritanceConnector

        private void RaiseMouseLeftButtonUpOnInheritanceConnectorEvent() {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = MouseLeftButtonUpOnInheritanceConnectorEvent;
            RaiseEvent(args);
        }

        public event RoutedEventHandler MouseLeftButtonUpOnInheritanceConnector {
            add { AddHandler(MouseLeftButtonUpOnInheritanceConnectorEvent, value); }
            remove { RemoveHandler(MouseLeftButtonUpOnInheritanceConnectorEvent, value); }
        }

        public static readonly RoutedEvent MouseLeftButtonUpOnInheritanceConnectorEvent = 
            EventManager.RegisterRoutedEvent(
                "MouseLeftButtonUpOnInheritanceConnector",
                RoutingStrategy.Bubble, 
                typeof(RoutedEventHandler), 
                typeof(UmlClassControl)
            );

        #endregion

        #region MouseLeftButtonUpOnCompositionConnector

        private void RaiseMouseLeftButtonUpOnCompositionConnectorEvent() {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = MouseLeftButtonUpOnCompositionConnectorEvent;
            RaiseEvent(args);
        }

        public event RoutedEventHandler MouseLeftButtonUpOnCompositionConnector {
            add { AddHandler(MouseLeftButtonUpOnCompositionConnectorEvent, value); }
            remove { RemoveHandler(MouseLeftButtonUpOnCompositionConnectorEvent, value); }
        }

        public static readonly RoutedEvent MouseLeftButtonUpOnCompositionConnectorEvent =
            EventManager.RegisterRoutedEvent(
                "MouseLeftButtonUpOnCompositionConnector",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(UmlClassControl)
            );

        #endregion

        #region MouseLeftButtonUpOnAssociationConnector

        private void RaiseMouseLeftButtonUpOnAssociationConnectorEvent() {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = MouseLeftButtonUpOnAssociationConnectorEvent;
            RaiseEvent(args);
        }

        public event RoutedEventHandler MouseLeftButtonUpOnAssociationConnector {
            add { AddHandler(MouseLeftButtonUpOnAssociationConnectorEvent, value); }
            remove { RemoveHandler(MouseLeftButtonUpOnAssociationConnectorEvent, value); }
        }

        public static readonly RoutedEvent MouseLeftButtonUpOnAssociationConnectorEvent =
            EventManager.RegisterRoutedEvent(
                "MouseLeftButtonUpOnAssociationConnector",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(UmlClassControl)
            );

        #endregion

        #region Public properties

        #region Name

        public string Name {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register(
                "Name",
                typeof(string),
                typeof(UmlClassControl),
                new FrameworkPropertyMetadata(null, OnNameChanged)
            );

        private static void OnNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var umlClassControl = (UmlClassControl) d;
            umlClassControl.OnNameChanged((string)e.OldValue, (string)e.NewValue);
        }

        private void OnNameChanged(string oldName, string newName) {
            UmlClass.Name = newName;
        }

        #endregion

        #region IsCurrentlyEdited

        public bool IsCurrentlyEdited {
            get { return (bool)GetValue(IsCurrentlyEditedProperty); }
            set { SetValue(IsCurrentlyEditedProperty, value); }
        }

        public static readonly DependencyProperty IsCurrentlyEditedProperty =
            DependencyProperty.Register(
                "IsCurrentlyEdited",
                typeof(bool),
                typeof(UmlClassControl),
                new FrameworkPropertyMetadata(
                    false, 
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
                )
            );

        #endregion

        #region ShowsOverlay

        public bool ShowsOverlay {
            get { return (bool)GetValue(ShowsOverlayProperty); }
            set { SetValue(ShowsOverlayProperty, value); }
        }

        public static readonly DependencyProperty ShowsOverlayProperty =
            DependencyProperty.Register(
                "ShowsOverlay",
                typeof(bool),
                typeof(UmlClassControl),
                new FrameworkPropertyMetadata(false)
            );

        #endregion

        #endregion

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            classNameTextBox = Template.FindName("PART_ClassNameTextBox", this) as TextBox;
            if (classNameTextBox != null) {
                classNameTextBox.KeyDown += OnKeyDown;
            }
            body = Template.FindName("PART_Body", this) as Border;
            inheritanceConnector = Template.FindName("PART_InheritanceConnector", this) as ContentControl;
            compositionConnector = Template.FindName("PART_CompositionConnector", this) as ContentControl;
            associationConnector = Template.FindName("PART_AssociationConnector", this) as ContentControl;

            return; // zoomandpancontrol will handle the mouse events and delegate them to umlcanvas

            if (body != null) {
                body.MouseLeftButtonUp +=
                    (sender, args) => {
                        RaiseMouseLeftButtonUpOnBodyEvent();
                        args.Handled = true;
                    };
            }

            if(inheritanceConnector != null) {
                inheritanceConnector.MouseLeftButtonUp +=
                    (sender, args) => {
                        RaiseMouseLeftButtonUpOnInheritanceConnectorEvent();
                        args.Handled = true;
                    };
            }

            if (compositionConnector != null) {
                compositionConnector.MouseLeftButtonUp +=
                    (sender, args) => {
                        RaiseMouseLeftButtonUpOnCompositionConnectorEvent();
                        args.Handled = true;
                    };
            }

            if (associationConnector != null) {
                associationConnector.MouseLeftButtonUp +=
                    (sender, args) => {
                        RaiseMouseLeftButtonUpOnAssociationConnectorEvent();
                        args.Handled = true;
                    };
            }
        }

        #region Private methods

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case "Name":
                    Name = UmlClass.Name;
                    break;
                case "BackgroundBrush":
                    Background = UmlClass.BackgroundBrush;
                    break;
                case "ForegroundBrush":
                    Foreground = UmlClass.ForegroundBrush;
                    break;
            }
        }

        private void OnAttributesChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if(e.Action == NotifyCollectionChangedAction.Reset) {
                Attributes.Clear();
            }
            if (e.OldItems != null) {
                foreach (UmlAttribute oldAttribute in e.OldItems) {
                    Attributes.Remove(oldAttribute);
                }
            }
            if (e.NewItems != null) {
                foreach (UmlAttribute newAttribute in e.NewItems) {
                    Attributes.Add(newAttribute);
                }
            }
        }

        private void OnOperationsChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Reset) {
                Operations.Clear();
            }
            if (e.OldItems != null) {
                foreach (UmlOperation oldOperation in e.OldItems) {
                    Operations.Remove(oldOperation);
                }
            }
            if (e.NewItems != null) {
                foreach (UmlOperation newOperation in e.NewItems) {
                    Operations.Add(newOperation);
                }
            }
        }

        private static void OnMouseDoubleClick(object sender, RoutedEventArgs e) {
            var ucc = e.Source as UmlClassControl;
            if (ucc != null) {
                ucc.IsCurrentlyEdited = true;
                e.Handled = true;
                if (ucc.classNameTextBox != null) {
                    ucc.Dispatcher.BeginInvoke(new Action(() => ucc.classNameTextBox.Focus()));
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                IsCurrentlyEdited = false; 
                e.Handled = true;
            }
        }

        private void MapNameToBackground() {
            Background = UmlClass.BackgroundBrush;
            Foreground = UmlClass.ForegroundBrush;
            if(Name.StartsWith("I") && char.IsUpper(Name[1])) {
                StereoType = "<<interface>>";
            }
        }
        #endregion
    }
}
