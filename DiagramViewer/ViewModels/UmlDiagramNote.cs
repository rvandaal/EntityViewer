
using System.Windows.Media;
using DiagramViewer.Models;

namespace DiagramViewer.ViewModels {
    public class UmlDiagramNote : DiagramNode {

        private readonly UmlNote note;

        public UmlDiagramNote(UmlNote umlNote) {
            note = umlNote;
        }

        public string Text {
            get { return note.Text; }
        }

        public Brush BackgroundBrush {
            get { return Brushes.Beige; }    
        }
    }
}
