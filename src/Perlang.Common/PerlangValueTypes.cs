#nullable enable
#pragma warning disable S1104
namespace Perlang;

public static class PerlangValueTypes
{
    public static readonly CppType Int32 = CppType.ValueType("int32_t", "int");
    public static readonly CppType UInt32 = CppType.ValueType("uint32_t", "uint");
    public static readonly CppType Int64 = CppType.ValueType("int64_t", "long");
    public static readonly CppType UInt64 = CppType.ValueType("uint64_t", "ulong");
    public static readonly CppType Float = CppType.ValueType("float", "float");
    public static readonly CppType Double = CppType.ValueType("double", "double");
    public static readonly CppType Bool = CppType.ValueType("bool", "bool");
    public static readonly CppType Char = CppType.ValueType("char16_t", "char"); // Deliberately not char on the C++-side, since it's an 8-bit type
    public static readonly CppType Enum = new CppType("PerlangEnum");
    public static readonly CppType Void = CppType.ValueType("void", "void");
    public static readonly CppType BigInt = CppType.ValueType("BigInt", "bigint");
}
