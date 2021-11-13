// TODO: Remove once https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3392 has been resolved
#pragma warning disable SA1515 // SingleLineCommentMustBePrecededByBlankLine

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using static Perlang.TokenType;

namespace Perlang.Parser
{
    /// <summary>
    /// Scans a Perlang program, converting it to a list of <see cref="Token"/>s.
    /// </summary>
    /// <remarks>
    /// Note that the documentation for this class talks about an "input stream" but in the current implementation, the
    /// whole program is read into a `String`. If this turns out to be inefficient, we may replace it with a
    /// stream-based implementation instead. An individual source file is unlikely to become much larger than a few
    /// thousand lines at most, so just using a plain `String` is probably the easiest, reasonable approach for now.
    /// </remarks>
    public class Scanner
    {
        public static readonly IDictionary<string, TokenType> ReservedKeywords =
            new Dictionary<string, TokenType>
            {
                { "and", AND },
                { "else", ELSE },
                { "false", FALSE },
                { "for", FOR },
                { "fun", FUN },
                { "if", IF },
                { "null", NULL },
                { "or", OR },
                { "print", PRINT },
                { "return", RETURN },
                { "super", SUPER },
                { "this", THIS },
                { "true", TRUE },
                { "var", VAR },
                { "while", WHILE },

                // Reserved keywords
                { "class", RESERVED_WORD }, // Pending #66 to be resolved.

                // Type names
                //
                // NOTE: only types not supported by
                // Perlang.Interpreter.Typing.TypeValidator.TypeResolver.ResolveExplicitTypes should be listed here.
                // Otherwise, we make it impossible to use them for variable declarations, return types etc.
                //
                { "byte", RESERVED_WORD },
                { "sbyte", RESERVED_WORD },
                { "short", RESERVED_WORD },
                { "ushort", RESERVED_WORD },
                { "uint", RESERVED_WORD },
                { "ulong", RESERVED_WORD },
                { "float", RESERVED_WORD },
                { "double", RESERVED_WORD },
                { "decimal", RESERVED_WORD },
                { "char", RESERVED_WORD },

                // Visibility, static/instance, etc
                { "public", RESERVED_WORD },
                { "private", RESERVED_WORD },
                { "protected", RESERVED_WORD },
                { "internal", RESERVED_WORD },
                { "static", RESERVED_WORD },
                { "volatile", RESERVED_WORD },

                // Standard functions
                { "printf", RESERVED_WORD },

                // Flow control
                { "switch", RESERVED_WORD },
                { "break", RESERVED_WORD },
                { "continue", RESERVED_WORD },

                // Exception handling
                { "try", RESERVED_WORD },
                { "catch", RESERVED_WORD },
                { "finally", RESERVED_WORD },

                // Asynchronous programming
                { "async", RESERVED_WORD },
                { "await", RESERVED_WORD },

                // Locking/synchronization
                { "lock", RESERVED_WORD },
                { "synchronized", RESERVED_WORD },

                // Others
                { "new", RESERVED_WORD },
                { "mut", RESERVED_WORD },
                { "let", RESERVED_WORD },
                { "const", RESERVED_WORD },
                { "struct", RESERVED_WORD },
                { "enum", RESERVED_WORD },
                { "sizeof", RESERVED_WORD },
                { "nameof", RESERVED_WORD },
                { "typeof", RESERVED_WORD },
                { "asm", RESERVED_WORD }
            }.ToImmutableDictionary();

        private static ISet<string> reservedKeywordStrings;

        public static ISet<string> ReservedKeywordStrings
        {
            get
            {
                reservedKeywordStrings ??= ReservedKeywords
                    .Where(kvp => kvp.Value == RESERVED_WORD)
                    .Select(kvp => kvp.Key)
                    .ToHashSet();

                return reservedKeywordStrings;
            }
        }

        private readonly string source;
        private readonly ScanErrorHandler scanErrorHandler;

        private readonly List<Token> tokens = new();
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
            if (Peek() == '#' && PeekNext() == '!')
            {
                // The input stream starts with a shebang (typically '#!/usr/bin/env perlang') line. The shebang
                // continues until the end of the line.
                while (Peek() != '\n' && !IsAtEnd())
                {
                    Advance();
                }
            }

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
                    if (Match('-'))
                    {
                        AddToken(MINUS_MINUS);
                    }
                    else if (Match('='))
                    {
                        AddToken(MINUS_EQUAL);
                    }
                    else
                    {
                        AddToken(MINUS);
                    }

                    break;
                case '+':
                    if (Match('+'))
                    {
                        AddToken(PLUS_PLUS);
                    }
                    else if (Match('='))
                    {
                        AddToken(PLUS_EQUAL);
                    }
                    else
                    {
                        AddToken(PLUS);
                    }

                    break;
                case ':':
                    AddToken(COLON);
                    break;
                case ';':
                    AddToken(SEMICOLON);
                    break;
                case '%':
                    AddToken(PERCENT);
                    break;
                case '*':
                    AddToken(Match('*') ? STAR_STAR : STAR);
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
                    // Even if the number is a number in a different base than 10 (binary, hexadecimal etc), it always
                    // starts with a "normal" (decimal) digit because of the prefix characters - e.g. 0x1234.
                    if (IsDigit(c, Base.DECIMAL))
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
            var numberStyles = NumberStyles.Any;
            var numberBase = Base.DECIMAL;
            int startOffset = 0;

            char currentChar = Char.ToLower(Peek());

            if (currentChar is 'b' or 'o' or 'x')
            {
                switch (currentChar)
                {
                    case 'b':
                        numberBase = Base.BINARY;
                        break;

                    case 'o':
                        numberBase = Base.OCTAL;
                        break;

                    case 'x':
                        numberStyles = NumberStyles.HexNumber;
                        numberBase = Base.HEXADECIMAL;
                        break;
                }

                // Moving the `start` pointer forward is important, since the parsing methods do not accept a prefix
                // like 0b or 0x being present. Adding a `startOffset` here feels safer than mutating `start`,
                // especially in case parsing fails somehow.
                Advance();
                startOffset = 2;
            }

            while (IsDigit(Peek(), numberBase) || Peek() == '_')
            {
                Advance();
            }

            // Look for a fractional part.
            if (Peek() == '.' && IsDigit(PeekNext(), numberBase))
            {
                isFractional = true;

                // Consume the "."
                Advance();

                while (IsDigit(Peek(), numberBase) || Peek() == '_')
                {
                    Advance();
                }
            }

            string numberCharacters = RemoveUnderscores(source[(start + startOffset)..current]);

            if (isFractional)
            {
                // TODO: This is a mess. We currently treat all floating point values as _double_, which is insane. We
                // TODO: should probably have a "use smallest possible type" logic as below for integers, for flotaing point
                // TODO: values as well. We could also consider supporting `decimal` while we're at it.
                AddToken(NUMBER, Double.Parse(numberCharacters));
            }
            else
            {
                // Any potential preceding '-' character has already been taken care of at this stage => we can treat
                // the number as an unsigned value. However, we still try to coerce it to the smallest signed or
                // unsigned integer type in which it will fit (but never smaller than 32-bit). This coincidentally
                // follows the same semantics as how C# does it, for simplicity.

                BigInteger value = numberBase switch
                {
                    Base.DECIMAL =>
                        BigInteger.Parse(numberCharacters, numberStyles),

                    Base.BINARY =>
                        Convert.ToUInt64(numberCharacters, 2),

                    Base.OCTAL =>
                        Convert.ToUInt64(numberCharacters, 8),

                    Base.HEXADECIMAL =>
                        // Quoting from
                        // https://docs.microsoft.com/en-us/dotnet/api/system.numerics.biginteger.parse?view=net-5.0#System_Numerics_BigInteger_Parse_System_ReadOnlySpan_System_Char__System_Globalization_NumberStyles_System_IFormatProvider_
                        //
                        // If value is a hexadecimal string, the Parse(String, NumberStyles) method interprets value as a
                        // negative number stored by using two's complement representation if its first two hexadecimal
                        // digits are greater than or equal to 0x80. In other words, the method interprets the highest-order
                        // bit of the first byte in value as the sign bit. To make sure that a hexadecimal string is
                        // correctly interpreted as a positive number, the first digit in value must have a value of zero.
                        //
                        // We presume that all hexadecimals should be treated as positive numbers for now.
                        BigInteger.Parse('0' + numberCharacters, numberStyles),

                    _ =>
                        throw new InvalidOperationException($"Base {(int)numberBase} not supported")
                };

                if (value < Int32.MaxValue)
                {
                    AddToken(NUMBER, (int)value);
                }
                else if (value < UInt32.MaxValue)
                {
                    AddToken(NUMBER, (uint)value);
                }
                else if (value < Int64.MaxValue)
                {
                    AddToken(NUMBER, (long)value);
                }
                else if (value < UInt64.MaxValue)
                {
                    AddToken(NUMBER, (ulong)value);
                }
                else // Anything else gets implicitly treated as BigInteger
                {
                    AddToken(NUMBER, value);
                }
            }
        }

        private static string RemoveUnderscores(string s)
        {
            var sb = new StringBuilder();

            foreach (char c in s.Where(c => c != '_'))
            {
                sb.Append(c);
            }

            return sb.ToString();
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

        /// <summary>
        /// Checks if the current character of the input stream matches the given character. If it matches, the
        /// character is consumed.
        /// </summary>
        /// <param name="expected">The character to look for.</param>
        /// <returns>`true` if the character matches, `false` if it doesn't matches or if we are at EOF.</returns>
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

        /// <summary>
        /// Returns the current character of the input stream, without advancing the current position.
        /// </summary>
        /// <returns>The character at the current position, or `\0` if at EOF.</returns>
        private char Peek()
        {
            if (IsAtEnd())
            {
                return '\0';
            }

            return source[current];
        }

        /// <summary>
        /// Returns the character immediately after the current character of the input stream, without advancing the
        /// current position.
        /// </summary>
        /// <returns>The character at the given position, or `\0` if at EOF.</returns>
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
            return (c is >= 'a' and <= 'z') ||
                   (c is >= 'A' and <= 'Z') ||
                   c is '_';
        }

        private static bool IsAlphaNumeric(char c) =>
            IsAlpha(c) || IsDigit(c, Base.DECIMAL);

        private static bool IsDigit(char c, Base @base) =>
            (int)@base switch
            {
                2 => c is '0' or '1',
                8 => c is >= '0' and <= '7',
                10 => c is >= '0' and <= '9',
                16 => c is >= '0' and <= '9' || (Char.ToUpper(c) >= 'A' && Char.ToUpper(c) <= 'F'),
                _ => throw new ArgumentException($"Base {@base} is not supported")
            };

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

        private enum Base
        {
            BINARY = 2,
            OCTAL = 8,
            DECIMAL = 10,
            HEXADECIMAL = 16,
        }
    }
}
