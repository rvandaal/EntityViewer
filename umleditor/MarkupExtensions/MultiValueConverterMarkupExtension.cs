using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Markup;
using System.Windows.Data;

namespace UmlEditor.MarkupExtensions {

    /// <summary>
    /// A generic markup extension for multi-value converters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class provides a generic way to construct markup extensions for multi-value converters.
    /// This is only relevant if your multi-value converter does not define any additional
    /// properties. If your multi-value converter does define additional properties, you are
    /// probably better off constructing your extension by deriving directly from
    /// <see cref="MarkupExtension"/>.
    /// </para>
    /// <para>
    /// To use this generic class, make sure that your multi-value converter is a class that has a
    /// default constructor, and that it implements the <see cref="IValueConverter"/> interface.
    /// </para>
    /// <para>
    /// There is also a version of this class for single-value converters,
    /// see <see cref="ConverterMarkupExtension&lt;T&gt;"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// The following shows how to use this class to create a markup extension for the
    /// (fictional) <c>FooMultiValueConverter</c>. Strictly speaking, it is not necessary to define
    /// the <see cref="MarkupExtensionReturnTypeAttribute"/>, but it does make the purpose
    /// of the extension more clear.
    /// <code lang="xml">
    /// <![CDATA[
    /// [MarkupExtensionReturnType(typeof(FooMultiValueConverter))]
    /// public sealed class FooMultiValueConverterExtension :
    ///     MultiValueConverterMarkupExtension<FooMultiValueConverter>
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="MarkupExtension">MarkupExtension Class</seealso>
    /// <seealso cref="IMultiValueConverter">IMultiValueConverter Interface</seealso>
    /// <seealso cref="ConverterMarkupExtension&lt;T&gt;">ConverterMarkupExtension Class</seealso>
    [SuppressMessage(
        "Microsoft.Naming",
        "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Multi",
        Justification = "Similar to Microsoft's IMultiValueConverter."
    )]
    public class MultiValueConverterMarkupExtension<T> : MarkupExtension
        where T : class, IMultiValueConverter, new() {

        private static T converter;

        /// <summary>
        /// Provides an instance of the multi-value converter that this class is an extension for.
        /// </summary>
        /// <param name="serviceProvider">
        /// An object that can provide services. Currently ignored.
        /// </param>
        /// <returns>
        /// The singleton instance of the multi-value converter that this class is an extension for.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider) {
            if (converter == null) {
                converter = new T();
            }

            return converter;
        }
    }
}