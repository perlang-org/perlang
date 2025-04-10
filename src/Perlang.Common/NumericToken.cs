using System.Globalization;

namespace Perlang;

public class NumericToken : Token
{
    public bool IsFractional { get; }
    public char? Suffix { get; }
    public Base NumberBase { get; }
    public NumberStyles NumberStyles { get; }
    public bool HasSuffix => Suffix != null;

    public NumericToken(string lexeme, string fileName, int line, string numberCharacters, char? suffix, bool isFractional, Base numberBase, NumberStyles numberStyles)
        : base(TokenType.NUMBER, lexeme, numberCharacters, fileName, line)
    {
        IsFractional = isFractional;
        Suffix = suffix;
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
