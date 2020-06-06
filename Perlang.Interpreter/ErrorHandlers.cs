using Perlang.Interpreter.Resolution;
using Perlang.Interpreter.Typing;

namespace Perlang.Interpreter
{
    public delegate void ResolveErrorHandler(ResolveError resolveError);
    public delegate void TypeValidationErrorHandler(TypeValidationError typeValidationError);
}
