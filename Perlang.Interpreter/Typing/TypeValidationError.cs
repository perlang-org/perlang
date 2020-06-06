using System.Collections.Generic;

namespace Perlang.Interpreter.Typing
{
    public class TypeValidationError : PerlangInterpreterException
    {
        /// <summary>
        /// The approximate location at which the error occurred.
        /// </summary>
        public Token Token { get; }

        public TypeValidationError(Token token, string message)
            : base(message)
        {
            Token = token;
        }
    }

    public class TypeValidationErrors : List<TypeValidationError>
    {
        public bool Empty() => Count == 0;
    }
}
