using System;

namespace Perlang.Parser
{
    public class ParseError : Exception
    {
        public Token Token { get; }
        public ParseErrorType? ParseErrorType { get; }

        public ParseError(string message, Token token, ParseErrorType? errorType) :
            base(message)
        {
            Token = token;
            ParseErrorType = errorType;
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

            return $"[line {Token.Line}] Error{where}: {Message}";
        }
    }
}
