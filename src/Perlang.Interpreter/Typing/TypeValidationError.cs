#nullable enable

namespace Perlang.Interpreter.Typing;

/// <summary>
/// Exception thrown on type validation errors.
/// </summary>
public class TypeValidationError : ValidationError
{
    public TypeValidationError(IToken token, string message)
        : base(token, message)
    {
    }
}