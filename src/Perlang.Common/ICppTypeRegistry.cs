#nullable enable
#pragma warning disable SA1117

using System.Collections.Generic;

namespace Perlang;

public interface ICppTypeRegistry
{
    CppType? Get(string cppTypeName);

    CppType? GetByPerlangTypeName(string perlangTypeName);

    CppType Register(
        string cppTypeName, string perlangTypeName, string? typeKeyword = null, bool wrapInSharedPtr = false,
        bool isSupported = true, bool isNullObject = false, bool isArray = false, bool isEnum = false, CppType? elementType = null,
        IEnumerable<IPerlangFunction>? extraMethods = null, IEnumerable<IPerlangField>? extraFields = null);
}
