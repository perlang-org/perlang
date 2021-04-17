using System;

namespace Perlang.Parser
{
    /// <summary>
    /// Represents a compiler warning.
    /// </summary>
    /// <remarks>
    /// Note that Warnings derive from <see cref="Exception"/>, even though they may or may not cause compilation to
    /// fail. It can make sense to think of them like "exceptions which may or may not be thrown", since that's exactly
    /// what they are.
    /// </remarks>
    public class CompilerWarning : Exception
    {
        public Token Token { get; }
        public WarningType WarningType { get; }

        public CompilerWarning(string message, Token token, WarningType warningType)
            : base(message)
        {
            Token = token;
            WarningType = warningType;
        }

        public override string ToString()
        {
            string where;

            if (Token.Type == TokenType.EOF)
            {
                where = " at end";
            }
            else
            {
                where = " at '" + Token.Lexeme + "'";
            }

            return $"[line {Token.Line}] Warning{where}: {Message}";
        }
    }
}
