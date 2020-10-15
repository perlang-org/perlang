namespace Perlang.Exceptions
{
    /// <summary>
    /// Abstract base class for exceptions thrown by stdlib.
    /// </summary>
    public abstract class StdlibException : RuntimeError
    {
        protected StdlibException(string message)
            : base(null, message)
        {
        }
    }
}
