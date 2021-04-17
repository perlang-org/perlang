#nullable enable

namespace Perlang.Interpreter.Immutability
{
    /// <summary>
    /// Exception thrown on immutability validation errors.
    /// </summary>
    public class ImmutabilityValidationError : ValidationError
    {
        public ImmutabilityValidationError(Token token, string message)
            : base(token, message)
        {
        }
    }
}
