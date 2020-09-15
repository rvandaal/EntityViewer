using System.Windows;
using DiagramViewer.ViewModels;

namespace DiagramViewer.Views {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindowViewModel ViewModel { get; private set; }
        public MainWindow(MainWindowViewModel viewModel) {
            ViewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
