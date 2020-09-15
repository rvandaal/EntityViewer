using System.Windows;
using UmlViewer.Controllers;
using UmlViewer.ViewModels;
using UmlViewer.Models;

namespace UmlViewer {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            var session = new Session();
            var mainWindowController = new MainWindowController(session);
            var mainWindowViewModel = new MainWindowViewModel(mainWindowController);
            var mainWindow = new MainWindow(mainWindowViewModel);
            mainWindow.Show();
        }
    }
}
