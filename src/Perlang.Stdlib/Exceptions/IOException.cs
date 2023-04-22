namespace Perlang.Exceptions;

/// <summary>
/// Exception thrown on IO errors.
/// </summary>
public class IOException : StdlibException
{
    public int ErrorNumber { get; }

    public IOException(string message, int errorNumber)
        : base($"Error {errorNumber}: {message}")
    {
        this.ErrorNumber = errorNumber;
    }
}
