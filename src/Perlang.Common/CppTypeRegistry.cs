#nullable enable
#pragma warning disable SA1010
#pragma warning disable SA1117

using System.Collections.Generic;
using Perlang.Compiler;

namespace Perlang;

public class CppTypeRegistry : ICppTypeRegistry
{
    private readonly Dictionary<string, CppType> registeredCppTypesByPerlangTypeName = [];

    public CppType? GetByPerlangTypeName(string perlangTypeName)
    {
        return registeredCppTypesByPerlangTypeName.GetValueOrDefault(perlangTypeName);
    }

    /// <inheritdoc/>
    public CppType Register(
        string cppTypeName, string perlangTypeName, string? typeKeyword = null, IEnumerable<CppType>? baseTypes = null,
        bool wrapInSharedPtr = false, bool isSupported = true, bool isNullObject = false, bool isArray = false,
        bool isEnum = false, bool isInterface = false, CppType? elementType = null, IEnumerable<IPerlangFunction>? extraMethods = null,
        IEnumerable<IPerlangField>? extraFields = null)
    {
        if (registeredCppTypesByPerlangTypeName.ContainsKey(perlangTypeName)) {
            throw new PerlangCompilerException($"Attempted to register type '{perlangTypeName}' which has already been registered");
        }

        var cppType = new CppType(
            cppTypeName, perlangTypeName, typeKeyword, baseTypes, wrapInSharedPtr, isSupported, isNullObject, isArray,
            isEnum, isInterface, elementType, extraMethods, extraFields
        );

        registeredCppTypesByPerlangTypeName[perlangTypeName] = cppType;

        return cppType;
    }
}
