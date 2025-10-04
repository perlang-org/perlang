#nullable enable

namespace Perlang.Interpreter.Internals;

public interface ITypeHandler
{
    void AddClass(string nameLexeme, IPerlangClass perlangClass);
    void AddEnum(string nameLexeme, PerlangEnum perlangEnum);
    IPerlangType? GetType(string name);
}
