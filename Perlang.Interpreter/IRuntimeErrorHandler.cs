namespace Perlang.Interpreter
{
    internal interface IRuntimeErrorHandler
    {
        void RuntimeError(RuntimeError error);
    }
}