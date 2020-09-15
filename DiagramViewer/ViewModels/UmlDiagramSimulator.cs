using System;
using System.Collections.ObjectModel;
using System.Windows;
using DiagramViewer.ViewModels.Forces;

namespace DiagramViewer.ViewModels {
    public class UmlDiagramSimulator : ViewModelBase {

        private readonly Diagram diagram;
        private NodeAttractionDefinition nodeAttractionDefinition;

        public UmlDiagramSimulator(Diagram diagram) {
            this.diagram = diagram;
            ForceDefinitions.Add(new Node2NodeRepulsionDefinition());
            ForceDefinitions.Add(new LinkAttractionDefinition());
            ForceDefinitions.Add(new NodeAttractionDefinition());
            ForceDefinitions.Add(new LinkMomentDefinition());
            ForceDefinitions.Add(new TagLaneCaptureDefinition());
        }

        private readonly ObservableCollection<ForceDefinition> forceDefinitions = new ObservableCollection<ForceDefinition>();
        public ObservableCollection<ForceDefinition> ForceDefinitions {
            get { return forceDefinitions; }
        }

        bool isSimulating = true;
        public bool IsSimulating {
            get { return isSimulating; }
            set { SetProperty(value, ref isSimulating, () => IsSimulating); }
        }

        private double fpsTime;
        private int fpsCount;
        private int lastFpsCount;

        private int fps;
        public int Fps {
            get { return fps; }
            set { SetProperty(value, ref fps, () => Fps); }
        }

        private double kineticEnergy;
        public double KineticEnergy {
            get { return kineticEnergy; }
            set { SetDoubleProperty(value, ref kineticEnergy, () => KineticEnergy); }
        }

        private void CalculateFps(double dt) {
            fpsTime += dt;
            fpsCount++;
            if (fpsTime > 1) {
                fpsTime = 0;
                lastFpsCount = fpsCount;
                fpsCount = 0;
            }
            Fps = lastFpsCount;
        }

        public void StartAttracting() {
            nodeAttractionDefinition = new NodeAttractionDefinition();
            ForceDefinitions.Add(nodeAttractionDefinition);
        }

        public void StopAttracting() {
            if (nodeAttractionDefinition != null) {
                ForceDefinitions.Remove(nodeAttractionDefinition);
                nodeAttractionDefinition = null;
            }
        }

        public void Simulate(double dt, double viewportWidth, double viewportHeight) {
            
            CalculateFps(dt);

            foreach (var diagramNode in diagram.Nodes) {
                //
                // The UpdateNeighbourConnectorRepulsionForces operates in the context of one node,
                // but adds a force to some other node. Although the rest of the updates doesn't influence other nodes,
                // this is better for performance, because we have less loops in loops in loops.
                // So we have to reset before all updates, otherwise we could get: node0 updates a force on node1, after that node1 resets.
                //
                diagramNode.ResetForces();
                diagramNode.UpdateForceMultiplier(dt);
            }

            foreach (var forceDefinition in ForceDefinitions) {
                forceDefinition.UpdateForces(diagram, viewportWidth, viewportHeight);
            }

            //// IDEA: use mousewheel to control the dt and therefore the stability of the simulation.
            //// Unstable? Scroll the mousewheel (in the good direction)

            if (isSimulating) {
                KineticEnergy = KineticEnergy > 1000000 ? UpdateAccVelPos(dt * 1) : UpdateAccVelPos(dt * 10);
                if (diagram.UmlDiagramInteractor.CurrentOperation == DragOperation.None) {
                    ApplyOffsetToCenter(viewportWidth, viewportHeight);
                }
            }
        }

        private double UpdateAccVelPos(double deltaTime) {
            double ke = 0.0;
            foreach (var class1 in diagram.UncontrolledNodes) {
                class1.Acc = class1.Force / class1.Mass;
                class1.Vel = (class1.Vel + class1.Acc * deltaTime) * GetDefaultDamping(class1);
                class1.Pos += class1.Vel * deltaTime;
                ke += 0.5 * class1.Mass * class1.Vel.LengthSquared;
            }
            return ke;
        }

        private double GetDefaultDamping(DiagramNode diagramNode) {
            return 0.85;
        }

        protected virtual void ApplyOffsetToCenter(double viewportWidth, double viewportHeight) {
            Vector leftTop = new Vector(double.PositiveInfinity, double.PositiveInfinity);
            foreach (var diagramNode in diagram.ForceExertingNodes) {
                leftTop.X = Math.Min(diagramNode.Pos.X - diagramNode.Size.Width / 2, leftTop.X);
                leftTop.Y = Math.Min(diagramNode.Pos.Y - diagramNode.Size.Height / 2, leftTop.Y);
            }

            foreach (var diagramNode in diagram.UncontrolledNodes) {
                diagramNode.Pos -= leftTop;
            }
        }
    }
}
