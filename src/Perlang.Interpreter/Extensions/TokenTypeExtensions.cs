using System;

namespace Perlang.Interpreter.Extensions
{
    /// <summary>
    /// Extension methods for IDictionary&lt;string, T&gt;.
    /// </summary>
    public static class TokenTypeExtensions
    {
        /// <summary>
        /// Converts the given <see cref="TokenType"/> to its representation in source code form.
        /// </summary>
        /// <param name="tokenType">A TokenType.</param>
        /// <returns>The source-level representation of the given token type.</returns>
        public static string ToSourceString(this TokenType tokenType)
        {
            return tokenType switch
            {
                TokenType.PLUS_EQUAL =>
                    "+=",
                TokenType.MINUS =>
                    "-",
                TokenType.MINUS_EQUAL =>
                    "-=",
                TokenType.SLASH =>
                    "/",
                TokenType.STAR =>
                    "*",
                TokenType.STAR_STAR =>
                    "**",
                TokenType.PERCENT =>
                    "%",
                _ =>
                    throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null)
            };
        }
    }
}
