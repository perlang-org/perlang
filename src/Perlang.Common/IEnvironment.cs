namespace Perlang
{
    public interface IEnvironment
    {
        void Define(Token name, object value);
        object GetAt(int distance, string name);
        void AssignAt(int distance, Token name, object value);
    }
}
