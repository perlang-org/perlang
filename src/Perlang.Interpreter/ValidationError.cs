#nullable enable

namespace Perlang.Interpreter;

/// <summary>
/// Base class for different kinds of validation errors.
/// </summary>
public abstract class ValidationError : PerlangInterpreterException
{
    /// <summary>
    /// Gets the approximate location at which the error occurred.
    /// </summary>
    public IToken Token { get; }

    protected ValidationError(IToken token, string message)
        : base(message)
    {
        Token = token;
    }
}
