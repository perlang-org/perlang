#nullable enable
namespace Perlang;

public interface IToken
{
    public TokenType Type { get; }
    public string Lexeme { get; }
    public object? Literal { get; }
    public string FileName { get; }
    public int Line { get; }
}
