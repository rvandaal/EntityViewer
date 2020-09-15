using System.Windows;
using System.Windows.Media;
using DiagramViewer.Models;
using DiagramViewer.Utilities;

namespace DiagramViewer.ViewModels {
    public class UmlDiagramNoteLink : DiagramLink {
        private Pen pen;

        internal UmlNoteLink NoteLink;
        public UmlDiagramClass Class { get { return (UmlDiagramClass)StartNode; } }
        public UmlDiagramNote Note { get { return (UmlDiagramNote)EndNode; } }

        public UmlDiagramNoteLink(UmlNoteLink umlNoteLink, UmlDiagramClass umlDiagramClass, UmlDiagramNote umlDiagramNote)
            : base(umlDiagramClass, umlDiagramNote) {
            NoteLink = umlNoteLink;
            IsVisible = true;
            pen = new Pen(new SolidColorBrush(Colors.Black), 1);
            pen.DashStyle = new DashStyle(new[] { 4.0, 4.0 }, 0.0);
        }

        public override void Draw(DrawingContext dc) {
            base.Draw(dc);
            dc.DrawLine(pen, StartConnectorPoint, EndConnectorPoint);
        }
    }
}
