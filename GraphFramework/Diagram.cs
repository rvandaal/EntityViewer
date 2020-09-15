
namespace GraphFramework {
    public class Diagram : Graph {
        protected override Node CreateNode() {
            return new Rectangle(this);
        }

        protected override Link CreateLink() {
            return new Connector();
        }
    }
}
