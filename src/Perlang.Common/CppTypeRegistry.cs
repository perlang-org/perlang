#nullable enable
#pragma warning disable SA1010
#pragma warning disable SA1117

using System.Collections.Generic;
using Perlang.Compiler;

namespace Perlang;

public class CppTypeRegistry : ICppTypeRegistry
{
    private readonly Dictionary<string, CppType> registeredCppTypes = [];

    public CppType? Get(string cppTypeName)
    {
        return registeredCppTypes.GetValueOrDefault(cppTypeName);
    }

    public CppType Register(
        string cppTypeName, string? perlangTypeName = null, string? typeKeyword = null, bool wrapInSharedPtr = false,
        bool isSupported = true, bool isNullObject = false, bool isArray = false, bool isEnum = false, CppType? elementType = null,
        IEnumerable<IPerlangFunction>? extraMethods = null, IEnumerable<IPerlangField>? extraFields = null)
    {
        if (registeredCppTypes.ContainsKey(cppTypeName)) {
            throw new PerlangCompilerException($"Attempted to register type '{cppTypeName}' which has already been registered");
        }

        return registeredCppTypes[cppTypeName] = new CppType(
            cppTypeName, perlangTypeName, typeKeyword, wrapInSharedPtr, isSupported, isNullObject, isArray, isEnum,
            elementType, extraMethods, extraFields
        );
    }

    public CppType GetOrRegister(string cppTypeName, string? perlangTypeName = null, string? typeKeyword = null, bool wrapInSharedPtr = false, bool isSupported = true, bool isNullObject = false, bool isArray = false, bool isEnum = false, CppType? elementType = null, IEnumerable<IPerlangFunction>? extraMethods = null, IEnumerable<IPerlangField>? extraFields = null)
    {
        return Get(cppTypeName) ??
               Register(cppTypeName, perlangTypeName, typeKeyword, wrapInSharedPtr, isSupported, isNullObject, isArray, isEnum, elementType, extraMethods, extraFields);
    }
}
