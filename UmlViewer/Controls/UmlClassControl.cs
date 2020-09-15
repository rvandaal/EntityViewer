using System.Windows;
using System.Windows.Controls;

namespace UmlViewer.Controls {
    public class UmlClassControl : Control {
        #region ClassName

        public string ClassName {
            get { return (string)GetValue(ClassNameProperty); }
            set { SetValue(ClassNameProperty, value); }
        }

        public static readonly DependencyProperty ClassNameProperty =
            DependencyProperty.Register("ClassName", typeof(string), typeof(UmlClassControl),
                new FrameworkPropertyMetadata(null)
            );

        #endregion


    }
}
