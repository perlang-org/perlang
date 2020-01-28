using System.Collections.Generic;

namespace Perlang.Interpreter
{
    public class ResolveError
    {
        public Token Token { get; set; }
        public string Message { get; set; }

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

    public class ResolveErrors : List<ResolveError>
    {
        public bool Empty() => Count == 0;
    }
}
