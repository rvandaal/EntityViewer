using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UmlViewer.Controls {

    public enum MouseOperation {
        None,
        MouseLeftButtonDownOnNode,
        MouseLeftButtonDownOnEmptySpace,
        MouseRightButtonDownOnNode,
        MouseRightButtonDownOnEmptySpace
    }

    public enum DragOperation {
        None,
        MovingNode,
        CreatingNode,
        CreatingLink
    }

    /// <summary>
    /// Custom <see cref="ListBox"/> that contains the different object which form a uml diagram.
    /// </summary>
    /// <remarks>
    /// An <see cref="UmlDiagramListBox"/> does not contain elements like a play/pause button.
    /// The style of the <see cref="UmlDiagramListBox"/> sets a <see cref="UmlCanvas"/> as its
    /// ItemsPanel.
    /// UmlCanvas doesn't know the itemcontainers so the mouse events for creating or moving an
    /// item container are best handled here, even though a UmlDiagramListBox could use an other 
    /// items panel in theory.
    /// </remarks>
    public class UmlDiagramListBox : ListBox {

        static UmlDiagramListBox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UmlDiagramListBox), new FrameworkPropertyMetadata(typeof(UmlDiagramListBox)));
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            var container = e.OriginalSource as UmlClassControl;
            if (container != null) {
                // Probleem: UmlCanvas kent de ItemContainers niet. Verantwoordelijkheid van de canvas
                // is puur layouting, dus hij heeft een compositie met de simulator.
                // Wat wel mogelijk is, is dat hij aan de listbox vraagt of een bepaalde source, een itemcontainer is.
            }
            else {
                
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e) {
            base.OnMouseRightButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e) {
            base.OnMouseRightButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
        }

        protected override System.Windows.DependencyObject GetContainerForItemOverride() {
            return new UmlClassControl();
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is UmlClassControl;
        }
    }
}
