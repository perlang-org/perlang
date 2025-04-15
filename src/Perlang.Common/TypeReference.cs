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
        private Type? clrType;
        private CppType? cppType;
        public Token? TypeSpecifier { get; }

        public Type? ClrType => clrType;

        public bool IsResolved =>
            cppType != null || ClrType != null;

        public void SetClrType(Type? value)
        {
            if (clrType != null && clrType != value) {
                // Poor-man's fake immutability. The problem is that the CLR type isn't really known at
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
                    $"ClrType already set to {clrType}; property is read-only. Attempted to " +
                    $"set it to {value}");
            }

            clrType = value;
        }

        public void SetCppType(CppType? value)
        {
            cppType = value;
        }

        public void SetPerlangClass(IPerlangClass? perlangClass) =>
            PerlangClass = perlangClass;

        private readonly bool isArray;

        public bool IsArray => clrType?.IsArray ?? isArray;

        public CppType? CppType =>
            cppType ?? clrType switch
            {
                // This is tested by "var_of_non_existent_type_with_initializer_emits_expected_error", in which case both
                // cppType and clrType will be null here.
                null => null,

                // Value types
                var t when t == typeof(Int32) => CppType.ValueType("int32_t"),
                var t when t == typeof(UInt32) => CppType.ValueType("uint32_t"),
                var t when t == typeof(Int64) => CppType.ValueType("int64_t"),
                var t when t == typeof(UInt64) => CppType.ValueType("uint64_t"),
                var t when t == typeof(Single) => CppType.ValueType("float"),
                var t when t == typeof(Double) => CppType.ValueType("double"),
                var t when t == typeof(bool) => CppType.ValueType("bool"),
                var t when t == typeof(void) => CppType.ValueType("void"),
                var t when t == typeof(BigInteger) => CppType.ValueType("BigInt"),
                var t when t == typeof(char) => CppType.ValueType("char16_t"), // Deliberately not char on the C++-side, since it's an 8-bit type

                // Arrays of value types
                var t when t == typeof(Int32[]) => new CppType("perlang::IntArray", WrapInSharedPtr: true),

                // Reference types
                var t when t.FullName == "Perlang.Lang.AsciiString" => new CppType("perlang::ASCIIString", WrapInSharedPtr: true),
                var t when t.FullName == "Perlang.Lang.String" => new CppType("perlang::String", WrapInSharedPtr: true),
                var t when t.FullName == "Perlang.Lang.Utf8String" => new CppType("perlang::UTF8String", WrapInSharedPtr: true),

                // Arrays of reference types
                var t when t.FullName == "Perlang.Lang.String[]" => new CppType("perlang::StringArray", WrapInSharedPtr: true),
                var t when t.FullName == "Perlang.Lang.AsciiString[]" => new CppType("perlang::StringArray", WrapInSharedPtr: true),
                var t when t.FullName == "Perlang.Lang.Utf8String[]" => new CppType("perlang::StringArray", WrapInSharedPtr: true),

                // These are not necessarily valid types on the C++ side, but must be handled to avoid the
                // NotImplementedInCompiledModeException exception below. We set them to something that we can use for
                // triggering a user-friendly exception at the PerlangCompiler stage.
                var t when t == typeof(string) => new CppType("string", IsSupported: false),
                var t when t == typeof(Type) => new CppType("Type", IsSupported: false),
                var t when t == typeof(PerlangEnum) => new CppType("PerlangEnum", IsSupported: false),
                var t when t == typeof(NullObject) => new CppType("NullObject", IsSupported: false),
                var t when t.FullName == "Perlang.Stdlib.Argv" => new CppType("perlang::Argv", IsSupported: false),
                var t when t.FullName == "Perlang.Stdlib.Libc" => new CppType("perlang::Libc", IsSupported: false),
                var t when t.FullName == "Perlang.Stdlib.Base64" => new CppType("perlang::Base64", IsSupported: false),
                var t when t.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(ImmutableDictionary<,>) => new CppType("ImmutableDictionary", IsSupported: false),

                _ => throw new NotImplementedInCompiledModeException($"Internal error: C++ type for {clrType} not defined")
            };

        public string? PossiblyWrappedCppType =>
            CppType?.PossiblyWrappedTypeName();

        public bool CppWrapInSharedPtr =>
            CppType?.WrapInSharedPtr ?? throw new PerlangCompilerException("Internal compiler error: cppType was unexpectedly null");

        public IPerlangClass? PerlangClass { get; private set; }

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
        /// Initializes a new instance of the <see cref="TypeReference"/> class, for a given CLR type.
        /// </summary>
        /// <param name="clrType">The CLR type.</param>
        public TypeReference(Type clrType)
        {
            // TODO: Remove once we are done with https://gitlab.perlang.org/perlang/perlang/-/issues/39
            this.clrType = clrType ?? throw new ArgumentException("clrType cannot be null");
        }

#pragma warning disable S3358
        public override string ToString()
        {
            var typeReference = (ITypeReference)this;

            if (typeReference.ExplicitTypeSpecified)
            {
                return typeReference.IsResolved ? $"Explicit: {(CppType != null ? CppType.TypeName : ClrType)}" : $"Explicit: {TypeSpecifier}";
            }
            else
            {
                return typeReference.IsResolved ? $"Inferred: {(CppType != null ? CppType.TypeName : ClrType)}" : "Inferred, not yet resolved";
            }
        }
#pragma warning restore S3358
    }
}
