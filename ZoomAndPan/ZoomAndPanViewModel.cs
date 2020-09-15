
namespace ZoomAndPan {
    public class ZoomAndPanViewModel : ViewModelBase {
        ///
        /// The current scale at which the content is being viewed.
        /// 
        private double contentScale = 1;

        ///
        /// The X coordinate of the offset of the viewport onto the content (in content coordinates).
        /// 
        private double contentOffsetX = 0;

        ///
        /// The Y coordinate of the offset of the viewport onto the content (in content coordinates).
        /// 
        private double contentOffsetY = 0;

        ///
        /// The width of the content (in content coordinates).
        /// 
        private double contentWidth = 0;

        ///
        /// The heigth of the content (in content coordinates).
        /// 
        private double contentHeight = 0;

        ///
        /// The width of the viewport onto the content (in content coordinates).
        /// The value for this is actually computed by the main window's ZoomAndPanControl and update in the
        /// data model so that the value can be shared with the overview window.
        /// 
        private double contentViewPortWidthInVC = 0;

        ///
        /// The heigth of the viewport onto the content (in content coordinates).
        /// The value for this is actually computed by the main window's ZoomAndPanControl and update in the
        /// data model so that the value can be shared with the overview window.
        /// 
        private double contentViewportHeight = 0;

        ///
        /// The current scale at which the content is being viewed.
        /// 
        public double ContentScale {
            get { return contentScale; }
            set { SetDoubleProperty(value, ref contentScale, () => ContentScale); }
        }

        ///
        /// The X coordinate of the offset of the viewport onto the content (in content coordinates).
        /// 
        public double ViewportOffsetXInCC {
            get { return contentOffsetX; }
            set {
                SetDoubleProperty(value, ref contentOffsetX, () => ViewportOffsetXInCC);
            }
        }

        ///
        /// The Y coordinate of the offset of the viewport onto the content (in content coordinates).
        /// 
        public double ViewportOffsetYInCC {
            get { return contentOffsetY; }
            set { SetDoubleProperty(value, ref contentOffsetY, () => ViewportOffsetYInCC); }
        }

        /// <summary>
        /// The width of the content (in content coordinates), the red rectangle.
        /// </summary>
        public double ContentWidth {
            get { return contentWidth; }
            set { SetDoubleProperty(value, ref contentWidth, () => ContentWidth); }
        }

        /// <summary>
        /// The heigth of the content (in content coordinates), the red rectangle.
        /// </summary>
        public double ContentHeight {
            get { return contentHeight; }
            set { SetDoubleProperty(value, ref contentHeight, () => ContentHeight); }
        }

        ///
        /// The width of the viewport onto the content (in content coordinates).
        /// The value for this is actually computed by the main window's ZoomAndPanControl and updated in the
        /// data model so that the value can be shared with the overview window.
        /// 
        public double ViewportWidthInCC {
            get { return contentViewPortWidthInVC; }
            set { SetDoubleProperty(value, ref contentViewPortWidthInVC, () => ViewportWidthInCC); }
        }

        ///
        /// The heigth of the viewport onto the content (in content coordinates).
        /// The value for this is actually computed by the main window's ZoomAndPanControl and updated in the
        /// data model so that the value can be shared with the overview window.
        /// 
        public double ViewportHeightInCC {
            get { return contentViewportHeight; }
            set { SetDoubleProperty(value, ref contentViewportHeight, () => ViewportHeightInCC); }
        }
    }
}
