#nullable enable

using System;
using System.Numerics;

namespace Perlang
{
    public interface ITypeReference
    {
        Token? TypeSpecifier { get; }

        /// <summary>
        /// Gets or sets the CLR/.NET type that this <see cref="ITypeReference"/> refers to.
        /// </summary>
        // TODO: Remove setter to make this interface and class be fully immutable, for debuggability.
        Type? ClrType { get; set; }

        /// <summary>
        /// Gets the C++ type that this <see cref="ITypeReference"/> refers to.
        /// </summary>
        string CppType { get; }

        /// <summary>
        /// Gets a cast to the C++ type that this <see cref="ITypeReference"/> refers to. Note that no validation if the
        /// cast will be possible or not is performed here; it is the responsibility of the caller.
        /// </summary>
        string CppTypeCast => $"({CppType})";

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

        bool IsStringType() =>
            ClrType switch
            {
                null => false,

                // Cannot use typeof(AsciiString) since Perlang.Common cannot depend on Perlang.Stdlib
                var t when t.FullName == "Perlang.Lang.AsciiString" => true,

                _ => false
            };
    }
}
