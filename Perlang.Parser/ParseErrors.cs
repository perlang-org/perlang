using System.Collections.Generic;

namespace Perlang.Parser
{
    public class ParseError
    {
        public Token Token { get; set; }
        public string Message { get; set; }
        public ParseErrorType? ParseErrorType { get; set; }

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

    public class ParseErrors : List<ParseError>
    {
        public bool Empty() => Count == 0;
    }
}
