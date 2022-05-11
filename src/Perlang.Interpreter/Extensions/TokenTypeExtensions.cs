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
            // I have thought about making a unit test for this (to ensure all token types are properly handled here),
            // but the problem is that we have token types like IDENTIFIER, STRING and so forth, which doesn't really
            // represent a constant "source string" in that sense. If we ever decide to group tokens in "constant" and
            // "variadic" groups or something, it could potentially be doable though. Even just "single-letter" and
            // "two-letter" groupings would be useful to begin with.

            return tokenType switch
            {
                TokenType.PLUS =>
                    "+",
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
                TokenType.LESS =>
                    "<",
                TokenType.LESS_EQUAL =>
                    "<=",
                TokenType.GREATER =>
                    ">",
                TokenType.GREATER_EQUAL =>
                    ">=",
                TokenType.LESS_LESS =>
                    "<<",
                TokenType.GREATER_GREATER =>
                    ">>",
                _ =>
                    throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null)
            };
        }
    }
}
