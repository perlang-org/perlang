#nullable enable
namespace Perlang.Interpreter.Internals;

public interface ITypeHandler
{
    void AddClass(string nameLexeme, IPerlangClass perlangClass);
}
