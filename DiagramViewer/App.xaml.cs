using System.Windows;
using DiagramViewer.ViewModels;
using DiagramViewer.Views;

namespace DiagramViewer {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            var mainWindowViewModel = new MainWindowViewModel();
            var mainWindow = new MainWindow(mainWindowViewModel);
            mainWindow.Show();
        }
    }
}
