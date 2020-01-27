using System.Collections.Generic;

namespace Perlang.Parser
{
    public class ParseError
    {
        public Token Token { get; set; }
        public string Message { get; set; }
        public ParseErrorType? ParseErrorType { get; set; }
    }

    public class ParseErrors : List<ParseError>
    {
        public bool Empty() => Count == 0;

        // Convenience method to free consumers from having to construct ScanErrors manually.
        public void Add(Token token, string message, ParseErrorType? parseErrorType)
        {
            Add(new ParseError { Token = token, Message = message, ParseErrorType = parseErrorType });
        }
    }
}