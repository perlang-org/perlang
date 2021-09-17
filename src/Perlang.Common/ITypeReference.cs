#nullable enable

using System;

namespace Perlang
{
    public interface ITypeReference
    {
        Token? TypeSpecifier { get; }

        // TODO: Remove setter to make this interface and class be fully immutable, for debuggability.
        Type? ClrType { get; set; }

        /// <summary>
        /// Gets a value indicating whether the type reference contains an explicit type specifier or not. If this is
        /// false, the user is perhaps intending for the type to be inferred from the program context.
        /// </summary>
        bool ExplicitTypeSpecified { get; }

        /// <summary>
        /// Gets a value indicating whether the type reference has been successfully resolved to a (loaded) CLR type or
        /// not.
        /// </summary>
        bool IsResolved { get; }
    }
}
