#nullable enable
#pragma warning disable SA1010
#pragma warning disable SA1118
#pragma warning disable SA1515
namespace Perlang;

/// <summary>
/// Standard Perlang types. See also <see cref="PerlangValueTypes"/> for a list of basic value types built into the
/// language.
/// </summary>
public static class PerlangTypes
{
    public static readonly CppType? NullObject = new CppType("NullObject", null, "null", isSupported: false);
    public static readonly CppType? PerlangObject = new CppType("PerlangObject", null, "object", wrapInSharedPtr: true);

    public static readonly CppType AsciiString = new CppType("perlang::ASCIIString", "ASCIIString", null, wrapInSharedPtr: true, extraFields: [
        new CppPropertyGetter("length", new TypeReference(PerlangValueTypes.Int64), methodName: "length")
    ]);

    public static readonly CppType String = new CppType("perlang::String", null, "string", wrapInSharedPtr: true);

    public static readonly CppType UTF8String = new CppType("perlang::UTF8String", "UTF8String", null, wrapInSharedPtr: true, extraMethods: [
        new CppFunction("as_utf16", parameters: [], new TypeReference(new Token(TokenType.IDENTIFIER, "UTF16String", literal: null, fileName: "", line: 0), isArray: false))
    ]);

    public static readonly CppType UTF16String = new CppType("perlang::UTF16String", "UTF16String", null, wrapInSharedPtr: true, extraFields: [
        // TODO: Support as_utf16() here too
        new CppPropertyGetter("length", new TypeReference(PerlangValueTypes.Int64), methodName: "length")
    ]);

    public static readonly CppType PerlangClass = new CppType("PerlangClass", "PerlangClass", wrapInSharedPtr: true);

    public static readonly CppType ASCIIStringArray = new CppType("perlang::ASCIIStringArray", "ASCIIString[]", null, wrapInSharedPtr: true, isArray: true, elementType: String);
    public static readonly CppType StringArray = new CppType("perlang::StringArray", null, "string[]", wrapInSharedPtr: true, isArray: true, elementType: String);
    public static readonly CppType UTF8StringArray = new CppType("perlang::UTF8StringArray", "UTF8String[]", null, wrapInSharedPtr: true, isArray: true, elementType: String);
    public static readonly CppType UTF16StringArray = new CppType("perlang::UTF16StringArray", "UTF16String[]", null, wrapInSharedPtr: true, isArray: true, elementType: String);

    // Note: should normally never be used directly, but rather new CppType() instances with the _same name_ as this
    // type should be created. This is needed because the elementType of the CppType instance needs to refer to the
    // real, concrete type being wrapped in an ObjectArray.
    public static readonly CppType ObjectArray = new CppType("perlang::ObjectArray", null, null, wrapInSharedPtr: true, isArray: true, elementType: PerlangObject);

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
