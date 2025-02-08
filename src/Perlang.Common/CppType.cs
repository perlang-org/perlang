#nullable enable
using Perlang.Compiler;

namespace Perlang;

public record CppType(string TypeName, bool WrapInSharedPtr = false, bool IsSupported = true)
{
    public static CppType ValueType(string typeName)
    {
        return new CppType(typeName, WrapInSharedPtr: false);
    }

    public string PossiblyWrappedTypeName()
    {
        if (!IsSupported)
        {
            throw new NotImplementedInCompiledModeException($"Wrapped type for {TypeName} is not supported in compiled mode");
        }

        return WrapInSharedPtr ? $"std::shared_ptr<{TypeName}>" : TypeName;
    }
}
