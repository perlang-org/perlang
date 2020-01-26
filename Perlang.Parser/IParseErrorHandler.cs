namespace Perlang.Parser
{
    public interface IParseErrorHandler
    {
        void ParseError(Token token, string message, ParseErrorType? parseErrorType);
    }
}