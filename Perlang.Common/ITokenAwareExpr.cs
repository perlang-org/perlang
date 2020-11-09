namespace Perlang
{
    /// <summary>
    /// Interface for expressions which provide a <see cref="Token"/>. This is used to augment error handling,for a
    /// better end-user experience.
    /// </summary>
    public interface ITokenAwareExpr
    {
        /// <summary>
        /// Gets the token that this expression represents, or a token close to it.
        /// </summary>
        public Token Token { get; }
    }
}
