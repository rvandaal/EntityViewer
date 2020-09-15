
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZoomAndPan;

namespace DiagramViewer.ViewModels {
    public class Diagram : ViewModelBase {

        public Diagram() {
            nodes = new ObservableCollection<DiagramNode>();
            Nodes = new ReadOnlyObservableCollection<DiagramNode>(nodes);
            links = new ObservableCollection<DiagramLink>();
            Links = new ReadOnlyObservableCollection<DiagramLink>(links);
            ZoomAndPanViewModel = new ZoomAndPanViewModel();
            UmlDiagramSimulator = new UmlDiagramSimulator(this);
            UmlDiagramInteractor = new UmlDiagramInteractor(this);
        }

        public ZoomAndPanViewModel ZoomAndPanViewModel { get; private set; }

        public UmlDiagramSimulator UmlDiagramSimulator { get; private set; }

        private bool showForces = true;
        public bool ShowForces {
            get { return showForces; }
            set { SetProperty(value, ref showForces, () => ShowForces); }
        }

        private bool isSimulating = true;
        public bool IsSimulating {
            get { return isSimulating; }
            set {
                if (SetProperty(value, ref isSimulating, () => IsSimulating)) {
                    if (value) {
                        ResumeSimulation();
                    } else {
                        PauseSimulation();
                    }
                }
            }
        }

        public void PauseSimulation() {
            UmlDiagramSimulator.IsSimulating = false;
        }

        public void ResumeSimulation() {
            UmlDiagramSimulator.IsSimulating = true;
        }

        public UmlDiagramInteractor UmlDiagramInteractor { get; private set; }

        #region Nodes

        public ReadOnlyObservableCollection<DiagramNode> Nodes { get; private set; }
        private readonly ObservableCollection<DiagramNode> nodes;

        public void AddNode(DiagramNode diagramNode) {
            if (!nodes.Contains(diagramNode)) {
                nodes.Add(diagramNode);
            }
        }

        public void RemoveNode(DiagramNode diagramNode) {
            if (nodes.Contains(diagramNode)) {
                nodes.Remove(diagramNode);
            }
        }

        #endregion

        #region Links

        public ReadOnlyObservableCollection<DiagramLink> Links { get; private set; }
        private readonly ObservableCollection<DiagramLink> links;

        public void AddLink(DiagramLink diagramLink) {
            if (!links.Contains(diagramLink)) {

                links.Add(diagramLink);
            }
        }

        public void RemoveLink(DiagramLink diagramLink) {
            if (links.Contains(diagramLink)) {
                links.Remove(diagramLink);
            }
        }

        #endregion

        public virtual void ClearDiagram() {
            nodes.Clear();
            links.Clear();
        }

        public IEnumerable<DiagramNode> UncontrolledNodes {
            get {
                return Nodes.Where(n => !n.IsPositionControlled);
            }
        }

        public IEnumerable<DiagramNode> ForceAcceptingNodes {
            get {
                return Nodes.Where(n => n.AcceptsForces);
            }
        }

        public IEnumerable<DiagramNode> ForceExertingNodes {
            get {
                return Nodes.Where(n => n.ExertsForces);
            }
        }

        public IEnumerable<DiagramNode> VisibleNodes {
            get {
                return Nodes.Where(n => n.IsVisible);
            }
        }

        public void Simulate(double dt, double viewportWidth, double viewportHeight) {
            UmlDiagramSimulator.Simulate(dt, viewportWidth, viewportHeight);
        }
    }
}
