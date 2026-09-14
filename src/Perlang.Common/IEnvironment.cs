namespace Perlang;

public interface IEnvironment
{
    void Define(IToken name, object value);
    object GetAt(int distance, string name);
    void AssignAt(int distance, IToken name, object value);
}