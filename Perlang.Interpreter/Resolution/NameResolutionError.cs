using Perlang.Interpreter.Typing;

namespace Perlang.Interpreter.Resolution
{
    public class NameResolutionError : TypeValidationError
    {
        public NameResolutionError(Token token, string message)
            : base(token, message)
        {
        }
    }
}
