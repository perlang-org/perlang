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
    public Token Token { get; }

    protected ValidationError(Token token, string message)
        : base(message)
    {
        Token = token;
    }
}