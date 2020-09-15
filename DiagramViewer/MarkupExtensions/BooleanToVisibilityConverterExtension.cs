using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace DiagramViewer.MarkupExtensions {
    /// <summary>
    /// Implements a markup extension that allows instances of WPF's
    /// <see cref="BooleanToVisibilityConverter"/> to be easily created.
    /// </summary>
    /// <remarks>
    /// This markup extension allows an instance of WPF's <see cref="BooleanToVisibilityConverter"/>
    /// to be easily created inline in a XAML binding. See the example below.
    /// </remarks>
    /// <example>
    /// The following shows how to use the <c>BooleanToVisibilityConverterExtension</c>
    /// inside a binding to convert a Boolean value to a <see cref="Visibility"/> value.
    /// <code lang="xml">
    /// <![CDATA[
    /// <uiElement Visibility="{Binding
    ///     SomeBooleanProperty,
    ///     Converter={ptc:BooleanToVisibilityConverter}}"/>
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="MarkupExtension">MarkupExtension Class</seealso>
    /// <seealso cref="BooleanToVisibilityConverter">BooleanToVisibilityConverter Class</seealso>
    /// <seealso cref="Visibility">Visibility Enumeration</seealso>
    [MarkupExtensionReturnType(typeof(BooleanToVisibilityConverter))]
    public sealed class BooleanToVisibilityConverterExtension :
        ConverterMarkupExtension<BooleanToVisibilityConverter> {
    }
}
