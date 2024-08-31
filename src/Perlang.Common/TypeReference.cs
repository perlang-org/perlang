#nullable enable
using System;
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
        public Token? TypeSpecifier { get; }

        public Type? ClrType => clrType;

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

        private readonly bool isArray;

        public bool IsArray => clrType?.IsArray ?? isArray;

        public string CppType =>
            clrType switch
            {
                null => throw new InvalidOperationException("Internal error: ClrType was unexpectedly null"),

                // Value types
                var t when t == typeof(Int32) => "int32_t",
                var t when t == typeof(UInt32) => "uint32_t",
                var t when t == typeof(Int64) => "int64_t",
                var t when t == typeof(UInt64) => "uint64_t",
                var t when t == typeof(Single) => "float",
                var t when t == typeof(Double) => "double",
                var t when t == typeof(bool) => "bool",
                var t when t == typeof(void) => "void",

                // Arrays of value types
                var t when t == typeof(Int32[]) => "perlang::IntArray",

                // Reference types
                var t when t == typeof(BigInteger) => "BigInt",

                // TODO: Handle UTF-8 strings here too
                var t when t.FullName == "Perlang.Lang.AsciiString" => "perlang::ASCIIString",
                var t when t.FullName == "Perlang.Lang.String" => "perlang::String",

                // Arrays of reference types
                var t when t.FullName == "Perlang.Lang.String[]" => "perlang::StringArray",
                var t when t.FullName == "Perlang.Lang.AsciiString[]" => "perlang::StringArray",
                var t when t.FullName == "Perlang.Lang.Utf8String[]" => "perlang::StringArray",

                _ => throw new NotImplementedInCompiledModeException($"Internal error: C++ type for {clrType} not defined")
            };

        public string PossiblyWrappedCppType =>
            clrType switch
            {
                null => throw new InvalidOperationException("Internal error: ClrType was unexpectedly null"),

                // Value types
                var t when t == typeof(Int32) => "int32_t",
                var t when t == typeof(UInt32) => "uint32_t",
                var t when t == typeof(Int64) => "int64_t",
                var t when t == typeof(UInt64) => "uint64_t",
                var t when t == typeof(Single) => "float",
                var t when t == typeof(Double) => "double",
                var t when t == typeof(bool) => "bool",
                var t when t == typeof(void) => "void",

                // Arrays of value types
                var t when t == typeof(Int32[]) => "std::shared_ptr<const perlang::IntArray>",

                // Reference types. BigInt is the outlier here, since it's actually used as a value type. This could
                // actually be one reason why the performance of using our BigInts are so bad; there may be implicit
                // copying happening behind the scenes.
                var t when t == typeof(BigInteger) => "BigInt",

                // These are wrapped in std::shared_ptr<>, as a simple way to deal with ownership for now. For the
                // long-term solution, see https://gitlab.perlang.org/perlang/perlang/-/issues/378.
                var t when t.FullName == "Perlang.Lang.AsciiString" => "std::shared_ptr<const perlang::ASCIIString>",
                var t when t.FullName == "Perlang.Lang.String" => "std::shared_ptr<const perlang::String>",
                var t when t.FullName == "Perlang.Lang.Utf8String" => "std::shared_ptr<const perlang::UTF8String>",

                // Arrays of reference types. Note how the array types are StringArray for all of these, because it is much
                // more complex to use different types here.
                var t when t.FullName == "Perlang.Lang.String[]" => "std::shared_ptr<const perlang::StringArray>",
                var t when t.FullName == "Perlang.Lang.AsciiString[]" => "std::shared_ptr<const perlang::StringArray>",
                var t when t.FullName == "Perlang.Lang.Utf8String[]" => "std::shared_ptr<const perlang::StringArray>",

                _ => throw new NotImplementedInCompiledModeException($"Internal error: C++ type for {clrType} not defined")
            };

        public bool CppWrapInSharedPtr =>
            clrType switch
            {
                null => throw new InvalidOperationException("Internal error: ClrType was unexpectedly null"),

                // Value types
                var t when t == typeof(Int32) => false,
                var t when t == typeof(UInt32) => false,
                var t when t == typeof(Int64) => false,
                var t when t == typeof(UInt64) => false,
                var t when t == typeof(Single) => false,
                var t when t == typeof(Double) => false,
                var t when t == typeof(bool) => false,
                var t when t == typeof(void) => false,
                var t when t == typeof(BigInteger) => true,

                // Arrays of value types
                var t when t == typeof(Int32[]) => true,

                // TODO: Handle UTF-8 strings here too
                var t when t.FullName == "Perlang.Lang.AsciiString" => true,
                var t when t.FullName == "Perlang.Lang.String" => true,

                // Arrays of reference types
                var t when t.FullName == "Perlang.Lang.AsciiString[]" => true,
                var t when t.FullName == "Perlang.Lang.String[]" => true,
                var t when t.FullName == "Perlang.Lang.Utf8String[]" => true,

                _ => throw new NotImplementedInCompiledModeException($"Internal error: C++ reference handling for {clrType} not defined")
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeReference"/> class, for a given type specifier. The type
        /// specifier can be null, in which case type inference will be attempted.
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

        public override string ToString()
        {
            var typeReference = (ITypeReference)this;

            if (typeReference.ExplicitTypeSpecified)
            {
                return typeReference.IsResolved ? $"Explicit: {ClrType}" : $"Explicit: {TypeSpecifier}";
            }
            else
            {
                return typeReference.IsResolved ? $"Inferred: {ClrType}" : "Inferred, not yet resolved";
            }
        }
    }
}
