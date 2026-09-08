#nullable enable
#pragma warning disable SA1117
#pragma warning disable SA1611

using System.Collections.Generic;
using Perlang.Compiler;

namespace Perlang;

public interface ICppTypeRegistry
{
    CppType? GetByPerlangTypeName(string perlangTypeName);

    /// <summary>
    /// Registers a new <see cref="CppType"/> in the registry.
    /// </summary>
    /// <returns>The newly registered <see cref="CppType"/> instance.</returns>
    /// <remarks>Note that this method will throw a <see cref="PerlangCompilerException"/> if the type is already
    /// registered.</remarks>
    /// <exception cref="PerlangCompilerException">A type with the given name has already been registered.</exception>
    CppType Register(
        string cppTypeName, string perlangTypeName, string? typeKeyword = null, IEnumerable<CppType>? baseTypes = null,
        bool wrapInSharedPtr = false, bool isSupported = true, bool isNullObject = false, bool isArray = false,
        bool isEnum = false, bool isInterface = false, bool isNullableUnion = false, CppType? elementType = null,
        IEnumerable<IPerlangFunction>? extraMethods = null, IEnumerable<IPerlangField>? extraFields = null);
}
