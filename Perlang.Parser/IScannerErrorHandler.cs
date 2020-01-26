namespace Perlang.Parser
{
    public interface IScannerErrorHandler
    {
        void ScannerError(int line, string unexpectedCharacter);
    }
}