namespace Perlang.Interpreter
{
    internal interface IResolveErrorHandler
    {
        void ResolveError(Token name, string message);
    }
}