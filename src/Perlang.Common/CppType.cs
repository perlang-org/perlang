#nullable enable
#pragma warning disable SA1010
#pragma warning disable SA1117

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Perlang.Compiler;

namespace Perlang;

public record CppType : IPerlangType
{
    public string Name => CppTypeName;

    // TODO: Should be an immutable Set instead for faster lookup, but preferably with Guava-style ImmutableMap
    // semantics (where iteration order == insertion order)
    public ImmutableList<IPerlangFunction> Methods { get; }
    public ImmutableList<IPerlangField> Fields { get; }

    /// <summary>
    /// Gets the C++ type name for this type.
    /// </summary>
    public string CppTypeName { get; }

    public string? PerlangTypeName { get; }

    public string? TypeKeyword { get; }
    public bool WrapInSharedPtr { get; }
    public bool IsSupported { get; }
    public bool IsNullObject { get; }
    public bool IsArray { get; }
    public bool IsEnum { get; }
    public CppType? ElementType { get; }

    public string TypeMethodNameSuffix => PerlangTypeName!.Replace(".", "_");

    public CppType(
        string cppTypeName, string? perlangTypeName = null, string? typeKeyword = null, bool wrapInSharedPtr = false,
        bool isSupported = true, bool isNullObject = false, bool isArray = false, bool isEnum = false, CppType? elementType = null,
        IEnumerable<IPerlangFunction>? extraMethods = null, IEnumerable<IPerlangField>? extraFields = null)
    {
#pragma warning disable CA2000
        this.Methods = new List<CppFunction>
        {
            // Deallocation of this gets handled by cleanup code in PerlangCompiler, by utilizing the TokenCleaner
            // IDisposable helper class.
            new CppFunction("get_type", parameters: [], new TypeReference(perlang_cli.CreateNullToken(TokenType.IDENTIFIER, "perlang::Type", file_name: "", line: 0), isArray: false))
        }.Concat(extraMethods ?? []).ToImmutableList();
#pragma warning restore CA2000

        this.CppTypeName = cppTypeName;
        this.PerlangTypeName = perlangTypeName;
        this.TypeKeyword = typeKeyword;
        this.WrapInSharedPtr = wrapInSharedPtr;
        this.IsSupported = isSupported;
        this.IsNullObject = isNullObject;
        this.IsArray = isArray;
        this.IsEnum = isEnum;

        if (isArray) {
            this.ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType), "Element type must be provided for array types");

            this.Fields = new List<PerlangField>
            {
                new PerlangField("length", new TypeReference(PerlangValueTypes.Int64))
            }.Concat(extraFields ?? []).ToImmutableList();
        }
        else {
            if (elementType != null) {
                throw new ArgumentException("Element type cannot be provided for non-array types", nameof(elementType));
            }

            this.ElementType = null;

            this.Fields = new List<PerlangField>()
                .Concat(extraFields ?? [])
                .ToImmutableList();
        }
    }

    public static CppType ValueType(string cppTypeName, string perlangTypeName, string typeKeyword)
    {
        return new CppType(cppTypeName, perlangTypeName, typeKeyword, wrapInSharedPtr: false);
    }

    public string PossiblyWrappedTypeName()
    {
        if (!IsSupported)
        {
            throw new NotImplementedInCompiledModeException($"Wrapped type for {CppTypeName} is not supported in compiled mode");
        }

        // TODO: Should this be const or not? Needed for make_shared_from_this(), but OTOH breaks string concatenation
        // since our stdlib doesn't expected const-qualified strings. I think we ended up not having to use
        // make_shared_from_this() so we can ignore this for now.
        return WrapInSharedPtr ? $"std::shared_ptr<{CppTypeName}>" : CppTypeName;
    }

    public bool IsAssignableTo(CppType? targetType)
    {
        if (targetType == PerlangTypes.String && (this == PerlangTypes.AsciiString || this == PerlangTypes.UTF8String)) {
            return true;
        }

        // Anything that is not `null` can be implicitly converted to `object`. The actual assignment might require some
        // conversion though, which is handled elsewhere.
        if (targetType == PerlangTypes.PerlangObject && this != PerlangTypes.NullObject) {
            return true;
        }

        // Let's do a super-simple implementation of this for starters. To be able to check subclassing/interfaces etc,
        // we'll need more type/reflection metadata I think.
        return this == targetType;
    }

    public CppType MakeArrayType()
    {
        return this switch
        {
            var t when t == PerlangValueTypes.Int32 => PerlangTypes.Int32Array,
            var t when t == PerlangValueTypes.UInt32 => PerlangTypes.UInt32Array,
            var t when t == PerlangValueTypes.Int64 => PerlangTypes.Int64Array,
            var t when t == PerlangValueTypes.UInt64 => PerlangTypes.UInt64Array,
            var t when t == PerlangValueTypes.Float => PerlangTypes.FloatArray,
            var t when t == PerlangValueTypes.Double => PerlangTypes.DoubleArray,
            var t when t == PerlangValueTypes.Char => PerlangTypes.CharArray,

            // Slightly weird, but using a single StringArray for now to avoid covariance issues.
            var t when t == PerlangTypes.AsciiString => PerlangTypes.StringArray,
            var t when t == PerlangTypes.String => PerlangTypes.StringArray,
            var t when t == PerlangTypes.UTF8String => PerlangTypes.StringArray,

            // Other types typically means array of user-defined type. We implement all of these as a generic
            // ObjectArray for simplicity, and can cast the individual elements to the more specific type as needed.
            var t => new CppType("perlang::ObjectArray", t.Name, wrapInSharedPtr: true, isArray: true, elementType: t)
        };
    }

    public CppType GetElementType()
    {
        if (!IsArray) {
            throw new InvalidOperationException("Only array types have an element type");
        }

        return ElementType ?? throw new InvalidOperationException("Element type unexpectedly null");
    }

    public virtual bool Equals(CppType? other)
    {
        if (ReferenceEquals(null, other)) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        // Ignoring Fields and Methods here for now, since we would need to implement equality for them and do a
        // Linq-style comparison for it.
        return CppTypeName == other.CppTypeName &&
               TypeKeyword == other.TypeKeyword &&
               WrapInSharedPtr == other.WrapInSharedPtr &&
               IsSupported == other.IsSupported &&
               IsNullObject == other.IsNullObject &&
               IsArray == other.IsArray &&
               Equals(ElementType, other.ElementType);
    }

    public override int GetHashCode()
    {
        var hashCode = default(HashCode);

        // Like above, ignoring Fields and Methods here for now
        hashCode.Add(CppTypeName);
        hashCode.Add(TypeKeyword);
        hashCode.Add(WrapInSharedPtr);
        hashCode.Add(IsSupported);
        hashCode.Add(IsNullObject);
        hashCode.Add(IsArray);
        hashCode.Add(ElementType);

        return hashCode.ToHashCode();
    }

    public override string ToString()
    {
        return $"{nameof(Name)}: {Name}, {nameof(Methods)}: [{String.Join(", ", Methods.Select(m => m.Name).ToList())}], {nameof(Fields)}: [{String.Join(", ", Fields.Select(f => f.Name).ToList())}], {nameof(TypeKeyword)}: {TypeKeyword}";
    }
}
