using System;
using Perlang.Interpreter.Internals;

namespace Perlang.Tests.TestHelpers;

internal class AssertFailAddTypeHandler : ITypeHandler
{
    public void AddClass(string name, IPerlangClass perlangClass)
    {
        throw new Exception($"Unexpected global class {name} attempted to be added. Global class: {perlangClass}");
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
