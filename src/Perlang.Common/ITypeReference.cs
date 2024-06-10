#nullable enable

using System;
using System.Numerics;

namespace Perlang
{
    public interface ITypeReference
    {
        Token? TypeSpecifier { get; }

        /// <summary>
        /// Gets the CLR/.NET type that this <see cref="ITypeReference"/> refers to.
        /// </summary>
        Type? ClrType { get; }

        /// <summary>
        /// Gets the C++ type that this <see cref="ITypeReference"/> refers to.
        /// </summary>
        string CppType { get; }

        /// <summary>
        /// Gets the C++ for this <see cref="ITypeReference"/>, possibly wrapped in a `std::shared_ptr&lt;T&gt;`.
        /// </summary>
        string PossiblyWrappedCppType { get; }

        /// <summary>
        /// Gets a cast to the C++ type that this <see cref="ITypeReference"/> refers to. Note that no validation if the
        /// cast will be possible or not is performed here; it is the responsibility of the caller.
        /// </summary>
        string CppTypeCast => $"({CppType})";

        /// <summary>
        /// Gets a value indicating whether this type should be wrapped in an `std::shared_ptr&lt;T&gt;` in certain cases
        /// (local variables, method parameters, etc).
        /// </summary>
        bool CppWrapInSharedPtr { get; }

        /// <summary>
        /// Gets a value indicating whether the type reference contains an explicit type specifier or not. If this is
        /// false, the user is perhaps intending for the type to be inferred from the program context.
        /// </summary>
        bool ExplicitTypeSpecified => TypeSpecifier != null;

        /// <summary>
        /// Gets a value indicating whether the type reference has been successfully resolved to a (loaded) CLR type or
        /// not.
        /// </summary>
        bool IsResolved => ClrType != null;

        /// <summary>
        /// Gets a value indicating whether this type reference refers to a `null` value.
        /// </summary>
        public bool IsNullObject => ClrType == typeof(NullObject);

        bool IsValidNumberType =>
            ClrType switch
            {
                null => false,
                var t when t == typeof(SByte) => true,
                var t when t == typeof(Int16) => true,
                var t when t == typeof(Int32) => true,
                var t when t == typeof(Int64) => true,
                var t when t == typeof(Byte) => true,
                var t when t == typeof(UInt16) => true,
                var t when t == typeof(UInt32) => true,
                var t when t == typeof(UInt64) => true,
                var t when t == typeof(Single) => true, // i.e. float
                var t when t == typeof(Double) => true,
                var t when t == typeof(BigInteger) => true,
                _ => false
            };

        bool IsStringType =>
            ClrType switch
            {
                null => false,

                // Cannot use typeof(AsciiString) since Perlang.Common cannot depend on Perlang.Stdlib
                var t when t.FullName == "Perlang.Lang.AsciiString" => true,
                var t when t.FullName == "Perlang.Lang.Utf8String" => true,
                var t when t.FullName == "Perlang.Lang.String" => true,

                _ => false
            };

        /// <summary>
        /// Sets the ClrType for the type reference. This method is typically called when type inference is performed.
        /// </summary>
        /// <remarks>This method may only be called a single time for a given <see cref="ITypeReference"/> instance.
        /// Calling it multiple times will emit an exception.</remarks>
        /// <param name="value">The new CLR (.NET) type for this type reference.</param>
        /// <exception cref="ArgumentException">The method is called when <see cref="ClrType"/> has already been
        /// set.</exception>
        void SetClrType(Type? value);
    }
}
