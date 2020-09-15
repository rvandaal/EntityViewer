namespace GraphFramework {
    public class Connector : Link{

        public Shape Shape1 { get { return (Shape)StartNode; } }
        public Shape Shape2 { get { return (Shape)EndNode; } }

        public double StartOffset {
            get {
                if (Shape1 != null) {
                    return Shape1.GetDistanceOfConnectorToEdge(this) / (EndPoint - StartPoint).Length;
                }
                return 0;
            }
        }
        public double EndOffset {
            get {
                if (Shape2 != null) {
                    return Shape2.GetDistanceOfConnectorToEdge(this) / (EndPoint - StartPoint).Length;
                }
                return 0;
            }
        }

        public Connector() {

        }

        public Connector(string preferredAngleString)
            : base(preferredAngleString) {
                
        }
    }
}
