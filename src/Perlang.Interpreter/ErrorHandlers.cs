using Perlang.Interpreter.Resolution;

namespace Perlang.Interpreter
{
    public delegate void ResolveErrorHandler(ResolveError resolveError);
    public delegate void ValidationErrorHandler(ValidationError typeValidationError);
}
