#nullable enable
#pragma warning disable S3440
using System;
using System.Collections.Immutable;
using System.Numerics;
using Perlang.Compiler;

namespace Perlang
{
    /// <summary>
    /// A type reference is a (potentially unresolved) reference to a Perlang type.
    ///
    /// The object is mutable.
    /// </summary>
    public class TypeReference : ITypeReference
    {
        public Token? TypeSpecifier { get; }

        public bool IsResolved =>
            CppType != null;

        public void SetCppType(CppType? value)
        {
            if (CppType != null && !CppType.Equals(value)) {
                // Poor-man's fake immutability. The problem is that the C++ type isn't really known at
                // construction time of the TypeReference; it is therefore always null. It gets populated
                // at a later stage in the compilation. But, how do we mutate it? We must either allow this
                // property to have a setter, or discard the TypeReference altogether and create a new one.
                // If we go the latter route, we just postpone the problem though; e.g. Stmt.Var.TypeReference
                // is not mutable either. So we would have to replace the Stmt node altogether, and so forth,
                // essentially discarding the whole AST for each node being handled by the type resolver.
                //
                // This is of course completely absurd and unreasonable. It has been said that an AST is a good
                // example of where immutability doesn't fit that well into the picture, and the above explanation
                // is one of the reasons why.
                //
                // So: the best we can do is to ensure that once the property is mutated, it is at least only
                // ever mutated _once_. I wonder if that will work all the way or if there are any other nice
                // gotchas to be discovered further along the road... :-)
                throw new ArgumentException(
                    $"CppType already set to {CppType}; property is read-only. Attempted to " +
                    $"set it to {value}");
            }

            CppType = value;
        }

        public void SetCppTypeFromClrType(Type clrType)
        {
            CppType = ClrTypeToCppType(clrType);
        }

        public void SetPerlangClass(IPerlangClass? perlangClass) =>
            PerlangClass = perlangClass;

        private readonly bool isArray;

        public bool IsArray => CppType?.IsArray ?? isArray;

        public CppType? CppType { get; private set; }

        public string? PossiblyWrappedCppType =>
            CppType?.PossiblyWrappedTypeName();

        public bool CppWrapInSharedPtr =>
            CppType?.WrapInSharedPtr ?? throw new PerlangCompilerException("Internal compiler error: cppType was unexpectedly null");

        public IPerlangClass? PerlangClass { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeReference"/> class, with no type specifier provided. This
        /// implies that type inference should be attempted.
        /// </summary>
        public TypeReference()
        {
            TypeSpecifier = null;
            isArray = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeReference"/> class, for a given type specifier. The type
        /// specifier can be <c>null</c>, in which case type inference will be attempted.
        /// </summary>
        /// <param name="typeSpecifier">The token providing the type specifier (e.g. 'int' or 'string').</param>
        /// <param name="isArray">Whether the type is an array or not.</param>
        public TypeReference(Token? typeSpecifier, bool isArray)
        {
            TypeSpecifier = typeSpecifier;
            this.isArray = isArray;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeReference"/> class, for a given <see cref="CppType"/>.
        /// </summary>
        /// <param name="cppType">The Perlang/C++ type.</param>
        public TypeReference(CppType cppType)
        {
            this.CppType = cppType ?? throw new ArgumentNullException(nameof(cppType), "cppType cannot be null");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeReference"/> class, for a given CLR type.
        /// </summary>
        /// <param name="clrType">The CLR type.</param>
        public TypeReference(Type clrType)
        {
            if (clrType == null) {
                throw new ArgumentException("clrType cannot be null");
            }

            this.CppType = ClrTypeToCppType(clrType);
        }

#pragma warning disable S3358
        public override string ToString()
        {
            var typeReference = (ITypeReference)this;

            if (typeReference.ExplicitTypeSpecified)
            {
                return typeReference.IsResolved ? $"Explicit: {CppType!.TypeName}" : $"Explicit: {TypeSpecifier}";
            }
            else
            {
                return typeReference.IsResolved ? $"Inferred: {CppType!.TypeName}" : "Inferred, not yet resolved";
            }
        }
#pragma warning restore S3358

        private static CppType? ClrTypeToCppType(Type clrType)
        {
            return clrType switch {
                // This is tested by "var_of_non_existent_type_with_initializer_emits_expected_error", in which case both
                // cppType and clrType will be null here.
                null => null,

                // Value types
                var t when t == typeof(Int32) => PerlangValueTypes.Int32,
                var t when t == typeof(UInt32) => PerlangValueTypes.UInt32,
                var t when t == typeof(Int64) => PerlangValueTypes.Int64,
                var t when t == typeof(UInt64) => PerlangValueTypes.UInt64,
                var t when t == typeof(Single) => PerlangValueTypes.Float,
                var t when t == typeof(Double) => PerlangValueTypes.Double,
                var t when t == typeof(bool) => PerlangValueTypes.Bool,
                var t when t == typeof(void) => PerlangValueTypes.Void,
                var t when t == typeof(BigInteger) => PerlangValueTypes.BigInt,
                var t when t == typeof(char) => PerlangValueTypes.Char,

                // Arrays of value types
                var t when t == typeof(Int32[]) => new CppType("perlang::IntArray", wrapInSharedPtr: true),

                // Reference types
                var t when t.FullName == "Perlang.Lang.AsciiString" => PerlangTypes.AsciiString,
                var t when t.FullName == "Perlang.Lang.String" => PerlangTypes.String,
                var t when t.FullName == "Perlang.Lang.Utf8String" => PerlangTypes.UTF8String,

                // Arrays of reference types. All of these become StringArray on the Perlang side; this is a bit of an approximation because C++ covar
                var t when t.FullName == "Perlang.Lang.AsciiString[]" => PerlangTypes.StringArray,
                var t when t.FullName == "Perlang.Lang.String[]" => PerlangTypes.StringArray,
                var t when t.FullName == "Perlang.Lang.Utf8String[]" => PerlangTypes.StringArray,

                var t when t == typeof(IPerlangClass) => PerlangTypes.PerlangClass,

                // These are not necessarily valid types on the C++ side, but must be handled to avoid the
                // NotImplementedInCompiledModeException exception below. We set them to something that we can use for
                // triggering a user-friendly exception at the PerlangCompiler stage.
                var t when t == typeof(string) => new CppType("string", isSupported: false),
                var t when t == typeof(Type) => new CppType("Type", isSupported: false),
                var t when t == typeof(PerlangEnum) => PerlangValueTypes.Enum,
                var t when t == typeof(NullObject) => new CppType("NullObject", isSupported: false, isNullObject: true),

                var t when t.FullName == "Perlang.Stdlib.Argv" => new CppType("perlang::Argv", isSupported: false),
                var t when t.FullName == "Perlang.Stdlib.Libc" => new CppType("perlang::Libc", isSupported: false),
                var t when t.FullName == "Perlang.Stdlib.Base64" => new CppType("perlang::Base64", isSupported: false),
                var t when t.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(ImmutableDictionary<,>) => new CppType("ImmutableDictionary", isSupported: false),

                _ => throw new NotImplementedInCompiledModeException($"Internal error: C++ type for {clrType} not defined")
            };
        }
    }
}
