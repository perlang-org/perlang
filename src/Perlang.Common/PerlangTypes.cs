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
    public static readonly CppType? PerlangObject = new CppType("perlang::Object", null, "object", wrapInSharedPtr: true);

    public static readonly CppType AsciiString = new CppType("perlang::ASCIIString", "ASCIIString", null, wrapInSharedPtr: true, extraFields: [
        new CppPropertyGetter("length", new TypeReference(PerlangValueTypes.Int64), methodName: "length")
    ]);

    public static readonly CppType String = new CppType("perlang::String", null, "string", wrapInSharedPtr: true);

    // TODO: Dispose of Token created here
    public static readonly CppType UTF8String = new CppType("perlang::UTF8String", "UTF8String", null, wrapInSharedPtr: true, extraMethods: [
        new CppFunction("as_utf16", parameters: [], new TypeReference(perlang_cli.CreateNullToken(TokenType.IDENTIFIER, "UTF16String", file_name: "", line: 0)))
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

    public static readonly CppType Int32Array = new CppType("perlang::IntArray", null, "int[]", wrapInSharedPtr: true, isArray: true, elementType: PerlangValueTypes.Int32);
    public static readonly CppType Int64Array = new CppType("perlang::LongArray", null, "long[]", wrapInSharedPtr: true, isArray: true, elementType: PerlangValueTypes.Int64);
    public static readonly CppType UInt32Array = new CppType("perlang::UIntArray", null, "uint[]", wrapInSharedPtr: true, isArray: true, elementType: PerlangValueTypes.UInt32);
    public static readonly CppType UInt64Array = new CppType("perlang::ULongArray", null, "ulong[]", wrapInSharedPtr: true, isArray: true, elementType: PerlangValueTypes.UInt64);
    public static readonly CppType FloatArray = new CppType("perlang::FloatArray", null, "float[]", wrapInSharedPtr: true, isArray: true, elementType: PerlangValueTypes.Float);
    public static readonly CppType DoubleArray = new CppType("perlang::DoubleArray", null, "double[]", wrapInSharedPtr: true, isArray: true, elementType: PerlangValueTypes.Double);
    public static readonly CppType BoolArray = new CppType("perlang::BoolArray", null, "bool[]", wrapInSharedPtr: true, isArray: true, elementType: PerlangValueTypes.Bool);
    public static readonly CppType CharArray = new CppType("perlang::CharArray", null, "char[]", wrapInSharedPtr: true, isArray: true, elementType: PerlangValueTypes.Char);
    public static readonly CppType BigIntArray = new CppType("perlang::BigIntArray", null, "bigint[]", wrapInSharedPtr: true, isArray: true, elementType: PerlangValueTypes.BigInt);

    public static readonly CppType Type = new CppType("perlang::Type", "perlang.Type", wrapInSharedPtr: true);
}
