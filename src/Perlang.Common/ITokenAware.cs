#nullable enable

namespace Perlang;

/// <summary>
/// Interface for expressions and statements which provide a <see cref="Token"/>. This is used to augment error
/// handling, for a better end-user experience.
/// </summary>
public interface ITokenAware
{
    /// <summary>
    /// Gets the token that this expression represents, or a token close to it. If `null`, the token is unknown.
    /// </summary>
    public IToken? Token { get; }
}
