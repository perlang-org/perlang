using System.Globalization;

namespace Perlang;

public class NumericToken : Token
{
    public bool IsFractional { get; }
    public Base NumberBase { get; }
    public NumberStyles NumberStyles { get; }

    public NumericToken(string lexeme, int line, string numberCharacters, bool isFractional, Base numberBase, NumberStyles numberStyles)
        : base(TokenType.NUMBER, lexeme, numberCharacters, line)
    {
        IsFractional = isFractional;
        NumberBase = numberBase;
        NumberStyles = numberStyles;
    }

    public enum Base
    {
        BINARY = 2,
        OCTAL = 8,
        DECIMAL = 10,
        HEXADECIMAL = 16,
    }
}
