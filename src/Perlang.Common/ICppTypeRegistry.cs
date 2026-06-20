#nullable enable
#pragma warning disable SA1117

using System.Collections.Generic;

namespace Perlang;

public interface ICppTypeRegistry
{
    CppType? Get(string cppTypeName);

    CppType Register(
        string cppTypeName, string? perlangTypeName = null, string? typeKeyword = null, bool wrapInSharedPtr = false,
        bool isSupported = true, bool isNullObject = false, bool isArray = false, bool isEnum = false, CppType? elementType = null,
        IEnumerable<IPerlangFunction>? extraMethods = null, IEnumerable<IPerlangField>? extraFields = null);

    CppType GetOrRegister(
        string cppTypeName, string? perlangTypeName = null, string? typeKeyword = null, bool wrapInSharedPtr = false,
        bool isSupported = true, bool isNullObject = false, bool isArray = false, bool isEnum = false, CppType? elementType = null,
        IEnumerable<IPerlangFunction>? extraMethods = null, IEnumerable<IPerlangField>? extraFields = null);
}
