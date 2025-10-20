using Perlang.Interpreter.NameResolution;

namespace Perlang.Interpreter.Typing;

/// <summary>
/// Emitted for name resolution errors detected while performing type validation.
/// </summary>
/// <remarks>
/// There are two phases during which name resolution errors may be detected:
///
/// - Name resolution (handled by the <see cref="NameResolver"/> class).
/// - Type validation (handled by the <see cref="TypeValidator"/> class).
///
/// This error class denoted errors detected during the latter of these phases.
/// </remarks>
public class NameResolutionTypeValidationError : TypeValidationError
{
    public NameResolutionTypeValidationError(IToken token, string message)
        : base(token, message)
    {
    }
}