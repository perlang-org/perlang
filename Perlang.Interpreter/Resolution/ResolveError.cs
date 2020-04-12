using System;

namespace Perlang.Interpreter.Resolution
{
    public class ResolveError : Exception
    {
        public Token Token { get; }

        public ResolveError(string message, Token token) :
            base(message)
        {
            Token = token;
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
