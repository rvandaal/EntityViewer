
namespace DiagramViewer.Models {
    public class UmlNote : Node {
        public string Text { get; set; }

        public UmlNote(string text) {
            Text = text;
        }
    }
}
