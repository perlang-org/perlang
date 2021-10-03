using Perlang.Interpreter.NameResolution;

namespace Perlang.Interpreter
{
    public delegate void NameResolutionErrorHandler(NameResolutionError nameResolutionError);
    public delegate void ValidationErrorHandler(ValidationError typeValidationError);
}
