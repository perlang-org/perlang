namespace Perlang.Parser
{
    public delegate void ParseErrorHandler(Token token, string message, ParseErrorType? parseErrorType);
}