using System;
using Perlang.Interpreter.Internals;

namespace Perlang.Tests.TestHelpers;

internal class AssertFailAddTypeHandler : ITypeHandler
{
    public void AddType(string name, IPerlangType perlangType)
    {
        throw new Exception($"Unexpected type {name} attempted to be added. Type: {perlangType}");
    }

    public void AddEnum(string name, PerlangEnum perlangEnum)
    {
        throw new Exception($"Unexpected enum {name} attempted to be added. Enum: {perlangEnum}");
    }

    public IPerlangType? GetType(string name)
    {
        return null;
    }
}
