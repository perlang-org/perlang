using Perlang.Interpreter;

namespace Perlang
{
    public interface IEnvironment
    {
        void Define(string name, Expr expr, object value);
        IBinding GetAt(int distance, string name);
        void AssignAt(int distance, Token name, Expr expr, object value);
    }
}
