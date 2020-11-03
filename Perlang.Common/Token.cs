#nullable enable
using System;

namespace Perlang
{
    public class Token
    {
        public TokenType Type { get; }
        public string Lexeme { get; }
        public object? Literal { get; }
        public int Line { get; }

        public Token(TokenType type, string lexeme, object? literal, int line)
        {
            // TODO: Replace with non-nullable references instead. https://github.com/perlang-org/perlang/issues/39
            Type = type;
            Lexeme = lexeme ?? throw new ArgumentException("lexeme cannot be null");
            Literal = literal;
            Line = line;
        }

        public override string ToString()
        {
            if (Literal != null)
            {
                return $"{Type} {Lexeme} {Literal}";
            }
            else
            {
                return $"{Type} {Lexeme}";
            }
        }
    }
}
