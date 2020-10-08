namespace Perlang.Interpreter.Typing
{
    public class TypeValidationError : PerlangInterpreterException
    {
        /// <summary>
        /// Gets the approximate location at which the error occurred.
        /// </summary>
        public Token Token { get; }

        public TypeValidationError(Token token, string message)
            : base(message)
        {
            Token = token;
        }
    }
}
