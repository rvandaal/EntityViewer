using System.Windows;
using System.Windows.Media;
using DiagramViewer.Models;
using DiagramViewer.Utilities;

namespace DiagramViewer.ViewModels
{
    public class UmlDiagramMethodLink : DiagramLink
    {
        private Pen pen;

        internal UmlMethodLink MethodLink;
        public UmlDiagramClass Class { get { return (UmlDiagramClass)StartNode; } }
        public UmlDiagramNote Note { get { return (UmlDiagramNote)EndNode; } }

        public UmlDiagramMethodLink(UmlMethodLink umlMethodLink, UmlDiagramClass umlDiagramClass, UmlDiagramMethodNode umlDiagramMethodNode)
            : base(umlDiagramClass, umlDiagramMethodNode)
        {
            MethodLink = umlMethodLink;
            IsVisible = true;
            pen = new Pen(new SolidColorBrush(Colors.Gray), 2);
            pen.DashStyle = new DashStyle(new[] { 4.0, 4.0 }, 0.0);
        }

        public override string Label { get { return "method"; } }

        public override void Draw(DrawingContext dc)
        {
            base.Draw(dc);
            dc.DrawLine(pen, StartConnectorPoint, EndConnectorPoint);
        }
    }
}
