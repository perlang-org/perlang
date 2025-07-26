#nullable enable
#pragma warning disable SA1010

using System;
using System.Collections.Immutable;
using System.Linq;
using Perlang.Compiler;

namespace Perlang;

public record CppType : IPerlangType
{
    public string Name => CppTypeName;

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
    public CppType? ElementType { get; }

    public string TypeMethodNameSuffix => PerlangTypeName!.Replace(".", "_");

    public CppType(string cppTypeName, string? perlangTypeName = null, string? typeKeyword = null, bool wrapInSharedPtr = false, bool isSupported = true, bool isNullObject = false, bool isArray = false, CppType? elementType = null)
    {
        this.Methods = [
            new CppFunction("get_type", parameters: [], new TypeReference(new Token(TokenType.IDENTIFIER, "perlang::Type", literal: null, fileName: String.Empty, line: 0), isArray: false))
        ];

        this.CppTypeName = cppTypeName;
        this.PerlangTypeName = perlangTypeName;
        this.TypeKeyword = typeKeyword ?? cppTypeName;
        this.WrapInSharedPtr = wrapInSharedPtr;
        this.IsSupported = isSupported;
        this.IsNullObject = isNullObject;
        this.IsArray = isArray;

        if (isArray) {
            this.ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType), "Element type must be provided for array types");
            this.Fields =
            [
                new PerlangField("length", new TypeReference(PerlangValueTypes.Int64))
            ];
        }
        else {
            if (elementType != null) {
                throw new ArgumentException("Element type cannot be provided for non-array types", nameof(elementType));
            }

            this.ElementType = null;
            this.Fields = [];
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

        return WrapInSharedPtr ? $"std::shared_ptr<{CppTypeName}>" : CppTypeName;
    }

    public bool IsAssignableTo(CppType? targetType)
    {
        if (targetType == PerlangTypes.String && (this == PerlangTypes.AsciiString || this == PerlangTypes.UTF8String)) {
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
            var t when t == PerlangValueTypes.BigInt => PerlangTypes.BigIntArray,

            // Slightly weird, but using a single StringArray for now to avoid covariance issues.
            var t when t == PerlangTypes.AsciiString => PerlangTypes.StringArray,
            var t when t == PerlangTypes.String => PerlangTypes.StringArray,
            var t when t == PerlangTypes.UTF8String => PerlangTypes.StringArray,

            _ => throw new NotImplementedInCompiledModeException($"Array type for {CppTypeName} is currently not supported")
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
        hashCode.Add(Methods);
        hashCode.Add(Fields);
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
