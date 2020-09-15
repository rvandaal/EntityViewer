namespace UmlEditor {
    public class MainWindowController {

        public MainWindowController() {
            Diagram = new UmlDiagram();
        }

        public UmlDiagram Diagram { get; private set; }
    }
}
