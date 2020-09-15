using System.Windows.Markup;
using UmlEditor.Converters;

namespace UmlEditor.MarkupExtensions {
    /// <summary>
    /// Implements a markup extension that allows instances of
    /// <see cref="AdditionConverter"/> to be easily created.
    /// </summary>
    /// <seealso cref="MarkupExtension">MarkupExtension Class</seealso>
    /// <seealso cref="AdditionConverter">AdditionConverter Class</seealso>
    [MarkupExtensionReturnType(typeof(AdditionConverter))]
    internal sealed class AdditionConverterExtension :
        ConverterMarkupExtension<AdditionConverter> {

    }
}
