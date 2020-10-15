using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using static Perlang.TokenType;

namespace Perlang.Parser
{
    public class Scanner
    {
        public static readonly IDictionary<string, TokenType> ReservedKeywords =
            new Dictionary<string, TokenType>
            {
                { "and", AND },
                { "class", CLASS },
                { "else", ELSE },
                { "false", FALSE },
                { "for", FOR },
                { "fun", FUN },
                { "if", IF },
                { "nil", NIL },
                { "or", OR },
                { "print", PRINT },
                { "return", RETURN },
                { "super", SUPER },
                { "this", THIS },
                { "true", TRUE },
                { "var", VAR },
                { "while", WHILE }
            }.ToImmutableDictionary();

        private readonly string source;
        private readonly ScanErrorHandler scanErrorHandler;

        private readonly List<Token> tokens = new List<Token>();
        private int start;
        private int current;
        private int line = 1;

        public Scanner(string source, ScanErrorHandler scanErrorHandler)
        {
            this.source = source;
            this.scanErrorHandler = scanErrorHandler;
        }

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                // We are at the beginning of the next lexeme.
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(EOF, System.String.Empty, null, line));
            return tokens;
        }

        private void ScanToken()
        {
            char c = Advance();

            switch (c)
            {
                case '(':
                    AddToken(LEFT_PAREN);
                    break;
                case ')':
                    AddToken(RIGHT_PAREN);
                    break;
                case '{':
                    AddToken(LEFT_BRACE);
                    break;
                case '}':
                    AddToken(RIGHT_BRACE);
                    break;
                case ',':
                    AddToken(COMMA);
                    break;
                case '.':
                    AddToken(DOT);
                    break;
                case '-':
                    AddToken(Match('-') ? MINUS_MINUS : MINUS);
                    break;
                case '+':
                    AddToken(Match('+') ? PLUS_PLUS : PLUS);
                    break;
                case ':':
                    AddToken(COLON);
                    break;
                case ';':
                    AddToken(SEMICOLON);
                    break;
                case '*':
                    AddToken(STAR);
                    break;
                case '!':
                    AddToken(Match('=') ? BANG_EQUAL : BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? EQUAL_EQUAL : EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? LESS_EQUAL : LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? GREATER_EQUAL : GREATER);
                    break;

                case '/':
                    if (Match('/'))
                    {
                        // A comment continues until the end of the line.
                        while (Peek() != '\n' && !IsAtEnd())
                        {
                            Advance();
                        }
                    }
                    else
                    {
                        AddToken(SLASH);
                    }

                    break;

                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;

                case '\n':
                    line++;
                    break;

                case '"':
                    String();
                    break;

                default:
                    if (IsDigit(c))
                    {
                        Number();
                    }
                    else if (IsAlpha(c))
                    {
                        Identifier();
                    }
                    else
                    {
                        scanErrorHandler(new ScanError("Unexpected character " + c, line));
                    }

                    break;
            }
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek()))
            {
                Advance();
            }

            // See if the identifier is a reserved word.
            string text = source[start..current];
            var type = ReservedKeywords.ContainsKey(text) ? ReservedKeywords[text] : IDENTIFIER;

            AddToken(type);
        }

        private void Number()
        {
            bool isFractional = false;

            while (IsDigit(Peek()))
            {
                Advance();
            }

            // Look for a fractional part.
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                isFractional = true;

                // Consume the "."
                Advance();

                while (IsDigit(Peek()))
                {
                    Advance();
                }
            }

            if (isFractional)
            {
                AddToken(NUMBER, Double.Parse(source[start..current]));
            }
            else
            {
                // Any potential preceding '-' character has already been taken care of at this stage => we can treat
                // the number as an unsigned value. However, we still try to coerce it to the smallest signed or
                // unsigned integer type in which it will fit (but never smaller than 32-bit). This coincidentally
                // follows the same semantics as how C# does it, for simplicity.
                ulong value = UInt64.Parse(source[start..current]);

                if (value < Int32.MaxValue)
                {
                    AddToken(NUMBER, (int) value);
                }
                else if (value < UInt32.MaxValue)
                {
                    AddToken(NUMBER, (uint) value);
                }
                else if (value < Int64.MaxValue)
                {
                    AddToken(NUMBER, (long) value);
                }
                else // ulong
                {
                    AddToken(NUMBER, value);
                }
            }
        }

        private void String()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n')
                {
                    line++;
                }

                Advance();
            }

            // Unterminated string.
            if (IsAtEnd())
            {
                scanErrorHandler(new ScanError("Unterminated string.", line));
                return;
            }

            // The closing ".
            Advance();

            // Trim the surrounding quotes.
            string value = source[(start + 1)..(current - 1)];
            AddToken(STRING, value);
        }

        private bool Match(char expected)
        {
            if (IsAtEnd())
            {
                return false;
            }

            if (source[current] != expected)
            {
                return false;
            }

            current++;
            return true;
        }

        private char Peek()
        {
            if (IsAtEnd())
            {
                return '\0';
            }

            return source[current];
        }

        private char PeekNext()
        {
            if (current + 1 >= source.Length)
            {
                return '\0';
            }

            return source[current + 1];
        }

        private static bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                   c == '_';
        }

        private static bool IsAlphaNumeric(char c) =>
            IsAlpha(c) || IsDigit(c);

        private static bool IsDigit(char c) =>
            c >= '0' && c <= '9';

        private bool IsAtEnd() =>
            current >= source.Length;

        private char Advance()
        {
            current++;
            return source[current - 1];
        }

        private void AddToken(TokenType type, object literal = null)
        {
            string text = source[start..current];
            tokens.Add(new Token(type, text, literal, line));
        }
    }
}
