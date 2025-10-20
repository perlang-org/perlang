using System.Globalization;

namespace Perlang;

// Note that NumericToken is currently implemented in C#, whereas the "normal" Token class used for other types has been
// rewritten in Perlang.
public class NumericToken : IToken
{
    public TokenType Type => TokenType.NUMBER;
    public string Lexeme { get; }
    public object Literal { get; }
    public string FileName { get; }
    public int Line { get; }

    public bool IsFractional { get; }
    public char? Suffix { get; }
    public Base NumberBase { get; }
    public NumberStyles NumberStyles { get; }
    public bool HasSuffix => Suffix != null;

    public NumericToken(string lexeme, string fileName, int line, string numberCharacters, char? suffix, bool isFractional, Base numberBase, NumberStyles numberStyles)
    {
        Lexeme = lexeme;
        Literal = numberCharacters;
        FileName = fileName;
        Line = line;

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
