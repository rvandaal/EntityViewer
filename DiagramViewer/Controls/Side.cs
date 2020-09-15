using System.ComponentModel;

namespace DiagramViewer.Controls {
    /// <summary>
    /// Enumeration defining the various sides (left, right, top, bottom) of a UI element.
    /// </summary>
    public enum Side {

        /// <summary>
        /// The left side.
        /// </summary>
        /// <remarks>
        /// The left side is considered to be the zero value for this enumeration.
        /// </remarks>
        [Description("Left")]
        Left = 0,

        /// <summary>
        /// The right side.
        /// </summary>
        [Description("Right")]
        Right,

        /// <summary>
        /// The top side.
        /// </summary>
        [Description("Top")]
        Top,

        /// <summary>
        /// The bottom side.
        /// </summary>
        [Description("Bottom")]
        Bottom
    }
}
