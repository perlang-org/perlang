namespace Perlang.Interpreter.Typing
{
    /// <summary>
    /// Exception thrown on type validation errors.
    /// </summary>
    public class TypeValidationError : ValidationError
    {
        public TypeValidationError(Token token, string message)
            : base(token, message)
        {
        }
    }
}
