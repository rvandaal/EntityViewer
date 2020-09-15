using System;
using System.Windows.Controls;
using GraphFramework;
using System.Windows;

namespace UmlEditor {
    public class NodeContainer2 : Decorator {
        public UmlClass UmlClass { get; private set; }
        public NodeContainer2(UmlClass umlClass) {
            UmlClass = umlClass;
            Update();
        }

        public void Update() {
            if(!double.IsNaN(UmlClass.TopLeft.X)) {
                Canvas.SetLeft(this, UmlClass.TopLeft2D.X);
                Canvas.SetTop(this, UmlClass.TopLeft2D.Y);
                MinWidth = UmlClass.Size2D.Width;
                MinHeight = UmlClass.Size2D.Height;       
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);
            UmlClass.Size2D = RenderSize;
        }
    }
}
