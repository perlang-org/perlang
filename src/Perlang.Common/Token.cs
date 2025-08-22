#nullable enable
using System;

namespace Perlang;

public class Token
{
    public TokenType Type { get; }
    public string Lexeme { get; }
    public object? Literal { get; }
    public string FileName { get; }
    public int Line { get; }

    public Token(TokenType type, string lexeme, object? literal, string fileName, int line)
    {
        // TODO: Replace with non-nullable references instead. https://gitlab.perlang.org/perlang/perlang/-/issues/39
        Type = type;
        Lexeme = lexeme ?? throw new ArgumentException("lexeme cannot be null");
        Literal = literal;
        FileName = fileName;
        Line = line;
    }

    public override string ToString()
    {
        if (Literal != null) {
            return $"{Type} {Lexeme} {Literal}";
        }
        else {
            return $"{Type} {Lexeme}";
        }
    }
}