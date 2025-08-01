#nullable enable
namespace Perlang;

/// <summary>
/// Standard Perlang types. See also <see cref="PerlangValueTypes"/> for a list of basic value types built into the
/// language.
/// </summary>
public static class PerlangTypes
{
    public static readonly CppType? NullObject = new CppType("NullObject", null, "null", isSupported: false);

    public static readonly CppType AsciiString = new CppType("perlang::ASCIIString", null, "ASCIIString", wrapInSharedPtr: true);
    public static readonly CppType String = new CppType("perlang::String", null, "string", wrapInSharedPtr: true);
    public static readonly CppType UTF8String = new CppType("perlang::UTF8String", null, "UTF8String", wrapInSharedPtr: true);

    public static readonly CppType PerlangClass = new CppType("PerlangClass", "PerlangClass", wrapInSharedPtr: true);

    public static readonly CppType StringArray = new CppType("perlang::StringArray", null, "string[]", wrapInSharedPtr: true, isArray: true, elementType: String);

    // TODO: int64_t and onwards should use perlang::<type> wrapper types too. Right now, attempting to use them will
    // TODO: likely cause compilation errors.
    public static readonly CppType Int32Array = new CppType("perlang::IntArray", null, "int[]", wrapInSharedPtr: true, isArray: true, elementType: PerlangValueTypes.Int32);
    public static readonly CppType Int64Array = new CppType("int64_t[]", null, "long[]", wrapInSharedPtr: true, isArray: true, elementType: PerlangValueTypes.Int64, isSupported: false);
    public static readonly CppType UInt32Array = new CppType("uint32_t[]", null, "uint[]", wrapInSharedPtr: true, isArray: true, elementType: PerlangValueTypes.UInt32, isSupported: false);
    public static readonly CppType UInt64Array = new CppType("uint64_t[]", null, "ulong[]", wrapInSharedPtr: true, isArray: true, elementType: PerlangValueTypes.UInt64, isSupported: false);
    public static readonly CppType FloatArray = new CppType("float[]", null, "float[]", wrapInSharedPtr: true, isArray: true, elementType: PerlangValueTypes.Float, isSupported: false);
    public static readonly CppType DoubleArray = new CppType("double[]", null, "double[]", wrapInSharedPtr: true, isArray: true, elementType: PerlangValueTypes.Double, isSupported: false);
    public static readonly CppType BoolArray = new CppType("bool[]", null, "bool[]", wrapInSharedPtr: true, isArray: true, elementType: PerlangValueTypes.Bool, isSupported: false);
    public static readonly CppType CharArray = new CppType("char16_t[]", null, "char[]", wrapInSharedPtr: true, isArray: true, elementType: PerlangValueTypes.Char, isSupported: false);
    public static readonly CppType BigIntArray = new CppType("BigInt[]", null, "bigint[]", wrapInSharedPtr: true, isArray: true, elementType: PerlangValueTypes.BigInt, isSupported: false);

    public static readonly CppType Type = new CppType("perlang::Type", "perlang.Type", wrapInSharedPtr: true);
}
