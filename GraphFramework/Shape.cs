namespace GraphFramework {
    public abstract class Shape : Node {
        protected Shape(Graph graph) : base(graph) {

        }

        public abstract double GetDistanceOfConnectorToEdge(Connector connector);
    }
}
