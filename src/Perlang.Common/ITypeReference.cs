#nullable enable

using System;
using Perlang.Compiler;

namespace Perlang
{
    public interface ITypeReference
    {
        Token? TypeSpecifier { get; }

        /// <summary>
        /// Gets a value indicating whether this type represents an array type, e.g. `string[]` or `int[]`.
        /// </summary>
        bool IsArray { get; }

        /// <summary>
        /// Gets the C++ type that this <see cref="ITypeReference"/> refers to. <c>null</c> means that type inference has
        /// not yet taken place.
        /// </summary>
        CppType? CppType { get; }

        /// <summary>
        /// Gets the C++ for this <see cref="ITypeReference"/>, possibly wrapped in a `std::shared_ptr&lt;T&gt;`.
        /// <c>null</c> means that type inference has not yet taken place.
        /// </summary>
        string? PossiblyWrappedCppType { get; }

        /// <summary>
        /// Gets a cast to the C++ type that this <see cref="ITypeReference"/> refers to. Note that no validation if the
        /// cast will be possible or not is performed here; it is the responsibility of the caller.
        /// </summary>
        /// <returns>A cast to the appropriate C++ type.</returns>
        string CppTypeCast()
        {
            if (CppType == null) {
                throw new PerlangCompilerException("Internal compiler error: Attempting to perform a type cast to unknown C++ type");
            }

            return $"({CppType.CppTypeName})";
        }

        /// <summary>
        /// Gets a value indicating whether this type should be wrapped in an `std::shared_ptr&lt;T&gt;` in certain cases
        /// (local variables, method parameters, etc.)
        /// </summary>
        bool CppWrapInSharedPtr { get; }

        /// <summary>
        /// Gets a value for the <see cref="PerlangType"/> that this <see cref="ITypeReference"/> refers to.
        /// </summary>
        IPerlangType? PerlangType { get; }

        /// <summary>
        /// Gets a value indicating whether the type reference contains an explicit type specifier or not. If this is
        /// false, the user is perhaps intending for the type to be inferred from the program context.
        /// </summary>
        bool ExplicitTypeSpecified => TypeSpecifier != null;

        /// <summary>
        /// Gets a value indicating whether the type reference has been successfully resolved to a type (CLR or C++) or
        /// not.
        /// </summary>
        bool IsResolved { get; }

        /// <summary>
        /// Gets a value indicating whether this type reference refers to a `null` value.
        /// </summary>
        public bool IsNullObject => CppType == PerlangTypes.NullObject;

        bool IsValidNumberType =>
            CppType switch
            {
                null => false,
                var t when t == PerlangValueTypes.Int32 => true,
                var t when t == PerlangValueTypes.UInt32 => true,
                var t when t == PerlangValueTypes.Int64 => true,
                var t when t == PerlangValueTypes.UInt64 => true,
                var t when t == PerlangValueTypes.BigInt => true,
                var t when t == PerlangValueTypes.Float => true,
                var t when t == PerlangValueTypes.Double => true,

                _ => false
            };

        bool IsStringType =>
            CppType switch
            {
                null => throw new InvalidOperationException("Internal error: Cannot determine if string type or not for null C++ type"),

                var t when t == PerlangTypes.AsciiString => true,
                var t when t == PerlangTypes.UTF8String => true,
                var t when t == PerlangTypes.String => true,

                _ => false
            };

        string TypeKeyword => CppType?.TypeKeyword ?? throw new InvalidOperationException($"Type keyword not defined for {CppType}");

        // Ensures anything but literal `null` get quoted
        string ToQuotedTypeKeyword() =>
            CppType == null || CppType == PerlangTypes.NullObject ?
            "null" :
            "'" + TypeKeyword + "'";

        /// <summary>
        /// Sets the C++ type for the type reference. This method is typically called when type inference is performed.
        /// </summary>
        /// <param name="value">The C++ type for this type reference.</param>
        /// <remarks>Note that for types which are typically wrapped in <c>std::shared_ptr&lt;T&gt;</c>, this refers to
        /// the unwrapped type. Wrapping will be handled by the <see cref="ITypeReference"/> implementation
        /// internally.</remarks>
        void SetCppType(CppType? value);

        void SetCppTypeFromClrType(Type clrType);

        void SetPerlangType(IPerlangType? perlangType);
    }
}
