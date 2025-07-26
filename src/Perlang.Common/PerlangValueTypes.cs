#nullable enable
#pragma warning disable S1104
namespace Perlang;

public static class PerlangValueTypes
{
    public static readonly CppType Int32 = CppType.ValueType("int32_t", "perlang.Int32", "int");
    public static readonly CppType Int64 = CppType.ValueType("int64_t", "perlang.Int64", "long");
    public static readonly CppType BigInt = CppType.ValueType("BigInt", "perlang.BigInt", "bigint");
    public static readonly CppType UInt32 = CppType.ValueType("uint32_t", "perlang.UInt32", "uint");
    public static readonly CppType UInt64 = CppType.ValueType("uint64_t", "perlang.UInt64", "ulong");
    public static readonly CppType Float = CppType.ValueType("float", "perlang.Float", "float");
    public static readonly CppType Double = CppType.ValueType("double", "perlang.Double", "double");
    public static readonly CppType Bool = CppType.ValueType("bool", "perlang.Bool", "bool");
    public static readonly CppType Char = CppType.ValueType("char16_t", "perlang.Char", "char"); // Deliberately not char on the C++-side, since it's an 8-bit type
    public static readonly CppType Enum = new CppType("PerlangEnum", "perlang.Enum");
    public static readonly CppType Void = CppType.ValueType("void", "perlang.Void", "void");
}
