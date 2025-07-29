#nullable enable

namespace Perlang.Interpreter.Internals;

public interface ITypeHandler
{
    void AddClass(string nameLexeme, IPerlangClass perlangClass);
    IPerlangType? GetType(string name);
}
