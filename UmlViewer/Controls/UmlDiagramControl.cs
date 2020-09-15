using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace UmlViewer.Controls {
    /// <summary>
    /// Custom control that comprises a whole class diagram.
    /// </summary>
    /// <remarks>
    /// This control also hides some properties of the <see cref="UmlDiagramListBox"/>.
    /// This control exposes UmlClasses and UmlRelations, not 'Items'.
    /// </remarks>
    public class UmlDiagramControl : Control { 

        static UmlDiagramControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UmlDiagramControl), new FrameworkPropertyMetadata(typeof(UmlDiagramControl)));
        }

        // This should define inheritable attached properties to which the UmlClassControls can bind. Properties like 
        // CurrentOperation='CreatingLink' should make the connectors visible on mouseover.

        public IEnumerable UmlClasses {
            get { return (IEnumerable)GetValue(UmlClassesProperty); }
            set { SetValue(UmlClassesProperty, value); }
        }

        public static readonly DependencyProperty UmlClassesProperty =
            DependencyProperty.Register("UmlClasses", typeof(IEnumerable), typeof(UmlDiagramControl), new FrameworkPropertyMetadata(null));
        

        
    }
}
