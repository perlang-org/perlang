namespace Perlang.Exceptions;

/// <summary>
/// Abstract base class for exceptions thrown by the Perlang standard library.
/// </summary>
public abstract class StdlibException : RuntimeError
{
    protected StdlibException(string message)
        : base(null, message)
    {
    }
}