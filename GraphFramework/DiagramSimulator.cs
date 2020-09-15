using System;
using System.Linq;
using System.Windows.Media.Media3D;

namespace GraphFramework {
    public class DiagramSimulator : GraphSimulator {
        protected override void ApplyOffsetToCenter(double viewportWidth, double viewportHeight) {
            //
            // To compute the boundarybox, a possible dragged node should also be included
            // (hence use VisibleNodes), but it should not have the offset applied (hence SimulatedNodes).
            //
            Vector3D leftTop = new Vector3D(double.PositiveInfinity, double.PositiveInfinity, 0);
            foreach (var rectangle in Graph.VisibleNodes.Cast<Rectangle>()) {
                leftTop.X = Math.Min(rectangle.Pos.X - rectangle.Size.Width / 2, leftTop.X);
                leftTop.Y = Math.Min(rectangle.Pos.Y - rectangle.Size.Height / 2, leftTop.Y);
            }
            //Debug.WriteLine(leftTop.ToString());

            foreach (var node in Graph.SimulatedNodes) {
                node.Pos -= leftTop;
            }
        }
    }
}
