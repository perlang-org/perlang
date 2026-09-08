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
    public List<CppType> BaseTypes { get; }
    public bool WrapInSharedPtr { get; }
    public bool IsSupported { get; }
    public bool IsNullObject { get; }
    public bool IsArray { get; }
    public bool IsEnum { get; }
    public bool IsInterface { get; }
    public bool IsNullableUnion { get; }
    public CppType? ElementType { get; }

    public string TypeMethodNameSuffix => PerlangTypeName!.Replace(".", "_");

    public CppType(string cppTypeName, string? perlangTypeName = null, string? typeKeyword = null,
        IEnumerable<CppType>? baseTypes = null, bool wrapInSharedPtr = false, bool isSupported = true,
        bool isNullObject = false, bool isArray = false, bool isEnum = false, bool isInterface = false,
        bool isNullableUnion = false, CppType? elementType = null, IEnumerable<IPerlangFunction>? extraMethods = null,
        IEnumerable<IPerlangField>? extraFields = null)
    {
#pragma warning disable CA2000
        if (isInterface) {
            this.Methods = (extraMethods ?? []).ToImmutableList();
            this.IsInterface = true;
        }
        else {
            this.Methods = new List<CppFunction>
            {
                // Deallocation of this gets handled by cleanup code in PerlangCompiler, by utilizing the TokenCleaner
                // IDisposable helper class.
                new CppFunction("get_type", parameters: [], new TypeReference(perlang_cli.CreateNullToken(TokenType.IDENTIFIER, "perlang::Type", file_name: "", line: 0), isArray: false))
            }.Concat(extraMethods ?? []).ToImmutableList();

            this.IsInterface = false;
        }
#pragma warning restore CA2000

        this.CppTypeName = cppTypeName;
        this.PerlangTypeName = perlangTypeName;
        this.TypeKeyword = typeKeyword;
        this.BaseTypes = (baseTypes ?? []).ToList();
        this.WrapInSharedPtr = wrapInSharedPtr;
        this.IsSupported = isSupported;
        this.IsNullObject = isNullObject;
        this.IsArray = isArray;
        this.IsEnum = isEnum;
        this.IsNullableUnion = isNullableUnion;

        if (isArray && isNullableUnion) {
            throw new ArgumentException("A type cannot be both an array type and a nullable union type", nameof(isNullableUnion));
        }

        if (isNullableUnion) {
            this.ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType), "Element type must be provided for nullable union types");

            this.Fields = new List<PerlangField>()
                .Concat(extraFields ?? [])
                .ToImmutableList();
        }
        else if (isArray) {
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
        // These are not registered in CppTypeRegistry. The calling class (e.g. PerlangValueTypes) will essentially
        // function as the "registry" for these types. The downside is that user-defined types which clashes with the
        // built-in types will not be as easily detected.
        return new CppType(cppTypeName, perlangTypeName, typeKeyword, wrapInSharedPtr: false);
    }

    public string PossiblyWrappedTypeName()
    {
        if (!IsSupported)
        {
            // TODO: Should use something like TypeKeywordOrPerlangType() here, for a better error message. This will
            // now refer to type names that are unusable on the Perlang side.
            throw new NotImplementedInCompiledModeException($"Wrapped type for {CppTypeName} is not supported in compiled mode");
        }

        // TODO: Should this be const or not? Needed for make_shared_from_this(), but OTOH breaks string concatenation
        // since our stdlib doesn't expected const-qualified strings. I think we ended up not having to use
        // make_shared_from_this() so we can ignore this for now.
        return WrapInSharedPtr ? $"std::shared_ptr<{CppTypeName}>" : CppTypeName;
    }

    // TODO: Should probably be made private. Outside callers should almost always call CanBeCoercedInto() instead,
    // which supports 'int' being assignable to 'long' and so forth.
    public bool IsAssignableTo(CppType targetType)
    {
        // Anything that is not `null` can be implicitly converted to `object`. The actual assignment might require some
        // conversion though, which is handled elsewhere.
        if (targetType == PerlangTypes.PerlangObject && this != PerlangTypes.NullObject) {
            return true;
        }

        // Assignment to the same type is always possible
        if (this == targetType) {
            return true;
        }

        return IsAssignableToHelper(this, targetType);
    }

    private bool IsAssignableToHelper(CppType obj, CppType targetType)
    {
        // Descend upwards in the type hierarchy, all the way to the root type(s), trying to find a common base type.
        if (obj.BaseTypes.Count != 0) {
            // We want to avoid a strict equality conversion here, since CppType instances can be created in multiple
            // places. Checking the (fully qualified) type name will have to do for now.
            if (obj.BaseTypes.Any(t => t.Name == targetType.Name)) {
                return true;
            }

            foreach (CppType baseType in this.BaseTypes) {
                bool result = IsAssignableToHelper(baseType, targetType);

                // As soon as we find a positive result, return to the caller, potentially breaking the recursion.
                if (result) {
                    return true;
                }
            }
        }

        return false;
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

    public CppType MakeNullableUnionType()
    {
        if (IsNullableUnion) {
            throw new InvalidOperationException($"{CppTypeName} is already a nullable union type");
        }

        return new CppType(
            $"std::optional<{PossiblyWrappedTypeName()}>",
            perlangTypeName: PerlangTypeName != null ? $"{PerlangTypeName} | null" : null,
            typeKeyword: TypeKeyword != null ? $"{TypeKeyword} | null" : null,
            wrapInSharedPtr: false,
            isNullableUnion: true,
            elementType: this
        );
    }

    public CppType GetElementType()
    {
        if (!IsArray && !IsNullableUnion) {
            throw new InvalidOperationException("Only array types and nullable union types have an element type");
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
               IsEnum == other.IsEnum &&
               IsInterface == other.IsInterface &&
               IsNullableUnion == other.IsNullableUnion &&
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
        hashCode.Add(IsEnum);
        hashCode.Add(IsInterface);
        hashCode.Add(IsNullableUnion);
        hashCode.Add(ElementType);

        return hashCode.ToHashCode();
    }

    public override string ToString()
    {
        return $"{nameof(Name)}: {Name}, {nameof(Methods)}: [{String.Join(", ", Methods.Select(m => m.Name).ToList())}], {nameof(Fields)}: [{String.Join(", ", Fields.Select(f => f.Name).ToList())}], {nameof(TypeKeyword)}: {TypeKeyword}";
    }
}
