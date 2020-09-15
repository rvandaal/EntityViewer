using System;
using System.Windows.Markup;
using System.Windows.Data;

namespace UmlEditor.MarkupExtensions {

    /// <summary>
    /// A generic markup extension for converters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class provides a generic way to construct markup extensions for converters.
    /// This is only relevant if your converter does not define any additional properties.
    /// If your converter does define additional properties, you are probably better off
    /// constructing your extension by deriving directly from <see cref="MarkupExtension"/>.
    /// </para>
    /// <para>
    /// To use this generic class, make sure that your converter is a class that has a
    /// default constructor, and that it implements the <see cref="IValueConverter"/> interface.
    /// </para>
    /// <para>
    /// There is also a version of this class for single-value converters,
    /// see <see cref="MultiValueConverterMarkupExtension&lt;T&gt;"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// The following shows how to use this class to create a markup extension for the
    /// (fictional) <c>FooConverter</c>. Strictly speaking, it is not necessary to define
    /// the <see cref="MarkupExtensionReturnTypeAttribute"/>, but it does make the purpose
    /// of the extension more clear.
    /// <code lang="xml">
    /// <![CDATA[
    /// [MarkupExtensionReturnType(typeof(FooConverter))]
    /// public sealed class FooConverterExtension :
    ///     ConverterMarkupExtension<FooConverter>
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="MarkupExtension">MarkupExtension Class</seealso>
    /// <seealso cref="IValueConverter">IValueConverter Interface</seealso>
    public class ConverterMarkupExtension<T> : MarkupExtension
        where T : class, IValueConverter, new() {

        private static T converter;

        /// <summary>
        /// Provides an instance of the converter that this class is an extension for.
        /// </summary>
        /// <param name="serviceProvider">
        /// An object that can provide services. Currently ignored.
        /// </param>
        /// <returns>
        /// The singleton instance of the converter that this class is an extension for.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider) {
            if (converter == null) {
                converter = new T();
            }

            return converter;
        }
    }
}