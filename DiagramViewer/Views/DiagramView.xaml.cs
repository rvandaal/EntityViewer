using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DiagramViewer.ViewModels;

namespace DiagramViewer.Views {
    /// <summary>
    /// Interaction logic for DiagramView.xaml
    /// </summary>
    public partial class DiagramView : UserControl {

        public DiagramView() {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            creatorAutoCompleteBox.Loaded += (sender, args) => creatorAutoCompleteBox.Focus();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (DataContext != null) {
                SubscribeForMouseEvents();
            }
        }

        private UmlDiagram UmlDiagram {
            get { return (UmlDiagram) DataContext; }
        }

        private void OnAutoCompleteBoxKeyUp(object sender, KeyEventArgs e) {
            if(e.Key == Key.Enter) {
                UmlDiagram.CommitInputText();
            }
        }

        private void SubscribeForMouseEvents() {
            //
            // This approach of subscribing to mouse events prevents the triggering of 
            // mousevents on the canvas when the canvas is resizing (this gives shakey classes).
            // Apart from this, the canvas can be smaller than the zoomandpancontrol. We want to 
            // allow classes to position themselves beyond the boundaries of the canvas.
            //
            zoomAndPanControl.MouseLeftButtonDown += canvas.OnMouseLeftButtonDown;
            zoomAndPanControl.MouseLeftButtonUp += canvas.OnMouseLeftButtonUp;
            zoomAndPanControl.MouseMove += canvas.OnMouseMove;
        }
    }
}
