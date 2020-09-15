using System.Windows;

namespace UmlEditor {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            var mainWindowController = new MainWindowController();
            var mainWindowViewModel = new MainWindowViewModel(mainWindowController);
            var mainWindow = new MainWindow(mainWindowViewModel);
            mainWindow.Show();
        }
    }
}
