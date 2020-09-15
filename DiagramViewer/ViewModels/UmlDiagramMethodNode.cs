
using System.Windows.Media;
using DiagramViewer.Models;

namespace DiagramViewer.ViewModels
{
    public class UmlDiagramMethodNode : DiagramNode
    {

        private readonly UmlMethodNode umlMethodNode;

        public UmlDiagramMethodNode(UmlMethodNode umlMethodNode)
        {
            this.umlMethodNode = umlMethodNode;
        }

        public string MethodName
        {
            get { return umlMethodNode.MethodName; }
        }

        public Brush BackgroundBrush
        {
            get { return Brushes.LightBlue; }
        }
    }
}
