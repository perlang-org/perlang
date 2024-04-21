// TODO: Remove once https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3392 has been resolved

#pragma warning disable SA1515 // SingleLineCommentMustBePrecededByBlankLine

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
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
        // NOTE: When making changes here, remember to adjust highlightjs-perlang.js also to ensure syntax highlighting
        // on the website matches the real set of keywords in the language.
        public static readonly IDictionary<string, TokenType> ReservedKeywords =
            new Dictionary<string, TokenType>
            {
                { "else", ELSE },
                { "false", FALSE },
                { "for", FOR },
                { "fun", FUN },
                { "if", IF },
                { "null", NULL },
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
                // NOTE: types supported by Perlang.Interpreter.Typing.TypeValidator.TypeResolver.ResolveExplicitTypes()
                // should not be listed here. Otherwise, we make it impossible to use them for variable declarations,
                // return types etc.
                //
                // When adding types to that method, the list below in `ReservedTypeKeywordStrings` also need to be
                // maintained. (This is indeed a rather unpleasant mess. It is done this way to allow `int`, `long` and
                // similar to be used as variable/parameter types, but forbid their usage as identifier names. One way
                // to fix this would by by defining INT, LONG etc as dedicated token types. Another way would be to add
                // some other flag here in addition to TokenType, to be able to more elegantly special-case it
                // elsewhere.)
                //
                { "byte", RESERVED_WORD },
                { "sbyte", RESERVED_WORD },
                { "short", RESERVED_WORD },
                { "ushort", RESERVED_WORD },
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

        public static IEnumerable<string> ReservedTypeKeywordStrings =>
            new List<string>
            {
                // Some type-related keywords are actually not marked as reserved words in ReservedKeywords,
                // since they are defined in Perlang.Interpreter.Typing.TypeResolver.ResolveExplicitTypes. We
                // special-case them here to make it easier to make these be proper reserved words sometime in
                // the future.
                "int",
                "long",
                "uint",
                "ulong",
                "bigint",
                "float",
                "double",
                "string"
            }.ToImmutableHashSet();

        private static ISet<string> reservedKeywordOnlyStrings;

        // Returns only the "proper" reserved keywords, not include the "reserved type keywords" listed above.
        public static ISet<string> ReservedKeywordOnlyStrings
        {
            get
            {
                reservedKeywordOnlyStrings ??= ReservedKeywords
                    .Where(kvp => kvp.Value == RESERVED_WORD)
                    .Select(kvp => kvp.Key)
                    .ToImmutableHashSet();

                return reservedKeywordOnlyStrings;
            }
        }

        public static IEnumerable<string> ReservedKeywordStrings =>
            ReservedKeywordOnlyStrings
                .Concat(ReservedTypeKeywordStrings)
                .ToImmutableHashSet();

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

            // Note: case values are sorted in ASCII order, which makes it easy and unambiguous to add new content.
            //
            // (Hmm, I'm having second thoughts about this... It actually makes it harder to find things in the code,
            // since things logically connected together like && and || end up being far apart. Might have to revisit
            // this again.)
            switch (c)
            {
                // Regular whitespace characters are ignored.
                case '\t':
                case '\r':
                case ' ':
                    break;

                // LFs are tracked to increase the newline count.
                case '\n':
                    line++;
                    break;

                case '!':
                    AddToken(Match('=') ? BANG_EQUAL : BANG);
                    break;
                case '"':
                    String();
                    break;

                case '#':
                    PreprocessorDirective();
                    break;

                case '%':
                    AddToken(PERCENT);
                    break;
                case '&':
                    AddToken(Match('&') ? AMPERSAND_AMPERSAND : AMPERSAND);
                    break;
                case '\'':
                    AddToken(SINGLE_QUOTE);
                    break;
                case '(':
                    AddToken(LEFT_PAREN);
                    break;
                case ')':
                    AddToken(RIGHT_PAREN);
                    break;
                case '*':
                    AddToken(Match('*') ? STAR_STAR : STAR);
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
                case ',':
                    AddToken(COMMA);
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
                case '.':
                    AddToken(DOT);
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

                // Digits handled in 'default' case.

                case ':':
                    AddToken(COLON);
                    break;
                case ';':
                    AddToken(SEMICOLON);
                    break;
                case '<':
                    if (Match('='))
                    {
                        AddToken(LESS_EQUAL);
                    }
                    else if (Match('<'))
                    {
                        AddToken(LESS_LESS);
                    }
                    else
                    {
                        AddToken(LESS);
                    }

                    break;
                case '=':
                    AddToken(Match('=') ? EQUAL_EQUAL : EQUAL);
                    break;
                case '>':
                    if (Match('='))
                    {
                        AddToken(GREATER_EQUAL);
                    }
                    else if (Match('>'))
                    {
                        AddToken(GREATER_GREATER);
                    }
                    else
                    {
                        AddToken(GREATER);
                    }

                    break;

                case '?':
                    AddToken(QUESTION_MARK);
                    break;

                // @ - unsure if this will ever be used. Java uses it for annotations.

                // Uppercase letters handled by 'default' case

                case '[':
                    AddToken(LEFT_SQUARE_BRACKET);
                    break;

                // \ could be used for continuing multiline strings, will we need it?

                case ']':
                    AddToken(RIGHT_SQUARE_BRACKET);
                    break;
                case '^':
                    AddToken(CARET);
                    break;

                // _ is handled by 'default' case, to allow identifiers containing underscore.

                // ` - unsure about this as well. F# uses it for "special function names", to make it possible to create
                // functions e.g. containing spaces. Markdown uses it for "code blocks". The disadvantage with the
                // backtick character is that in some non-US keyboard layouts, it is a "dead" key, requiring multiple
                // keypresses to type it.

                // 'a-z' are handled by 'default' case.

                case '{':
                    AddToken(LEFT_BRACE);
                    break;
                case '|':
                    AddToken(Match('|') ? PIPE_PIPE : PIPE);
                    break;
                case '}':
                    AddToken(RIGHT_BRACE);
                    break;

                // All ASCII characters not handled above + all other non-ASCII (Unicode) characters
                default:
                    // Even if a number is specified in a different base than 10 (e.g. binary, hexadecimal etc), it
                    // always starts with a "normal" (decimal) digit because of the prefix characters - e.g. 0x1234.
                    if (IsDigit(c, NumericToken.Base.DECIMAL))
                    {
                        Number();
                    }
                    else if (IsAlpha(c) || IsUnderscore(c))
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
            var numberBase = NumericToken.Base.DECIMAL;
            int startOffset = 0;

            char currentChar = Char.ToLower(Peek());

            if (currentChar is 'b' or 'o' or 'x')
            {
                switch (currentChar)
                {
                    case 'b':
                        numberBase = NumericToken.Base.BINARY;
                        break;

                    case 'o':
                        numberBase = NumericToken.Base.OCTAL;
                        break;

                    case 'x':
                        numberStyles = NumberStyles.HexNumber;
                        numberBase = NumericToken.Base.HEXADECIMAL;
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
            char? suffix = null;

            if (IsAlpha(Peek()))
            {
                suffix = Advance();
            }

            // Note that numbers are not parsed at this stage. We deliberately postpone it to the parsing stage, to be
            // able to conjoin MINUS and NUMBER tokens together for negative numbers. The previous approach (inherited
            // from Lox) worked poorly with our idea of "narrowing down" constants to smallest possible integer. See
            // #302 for some more details.
            AddToken(new NumericToken(source[start..current], line, numberCharacters, suffix, isFractional, numberBase, numberStyles));
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

        // "Preprocessor directives" are a bad name here, but calling it this for lack of better wording. "Macros" would
        // be one option, but the directives we currently support are in fact closer to preprocessor-like directives.
        private void PreprocessorDirective()
        {
            while (Peek() != '\n' && !IsAtEnd()) {
                Advance();
            }

            // TrimEnd() call needed to workaround Windows CR+LF line endings
            string startDirective = source[(start + 1)..current].TrimEnd();
            int valueStart = current;

            switch (startDirective) {
                case "c++-prototypes":
                {
                    while (Peek() != '#' && PeekNext() != '/' && !IsAtEnd()) {
                        Advance();
                    }

                    if (IsAtEnd()) {
                        scanErrorHandler(new ScanError($"Unterminated preprocessor directive {startDirective}.", line));
                        return;
                    }

                    // Consume the '#'
                    Advance();

                    int endDirectiveStart = current;

                    while (Peek() != '\n' && !IsAtEnd()) {
                        Advance();
                    }

                    string endDirective = source[endDirectiveStart..current].Trim();

                    if (endDirective == "/c++-prototypes") {
                        string value = source[valueStart..(endDirectiveStart - 1)];
                        AddToken(PREPROCESSOR_DIRECTIVE_CPP_PROTOTYPES, value.Trim());
                    }
                    else {
                        scanErrorHandler(new ScanError($"Expected '/c++-prototypes' but got '{endDirective}'.", line));
                    }

                    break;
                }

                case "c++-methods":
                {
                    while (Peek() != '#' && PeekNext() != '/' && !IsAtEnd()) {
                        Advance();
                    }

                    if (IsAtEnd()) {
                        scanErrorHandler(new ScanError($"Unterminated preprocessor directive {startDirective}.", line));
                        return;
                    }

                    // Consume the '#'
                    Advance();

                    int endDirectiveStart = current;

                    while (Peek() != '\n' && !IsAtEnd()) {
                        Advance();
                    }

                    string endDirective = source[endDirectiveStart..current].Trim();

                    if (endDirective == "/c++-methods") {
                        string value = source[valueStart..(endDirectiveStart - 1)];
                        AddToken(PREPROCESSOR_DIRECTIVE_CPP_METHODS, value.Trim());
                    }
                    else {
                        scanErrorHandler(new ScanError($"Expected '/c++-methods' but got '{endDirective}'.", line));
                    }

                    break;
                }

                default:
                    scanErrorHandler(new ScanError($"Unknown preprocessor directive {startDirective}.", line));
                    break;
            }
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
                   (c is >= 'A' and <= 'Z');
        }

        private static bool IsUnderscore(char c) =>
            c is '_';

        private static bool IsAlphaNumeric(char c) =>
            IsAlpha(c) || IsUnderscore(c) || IsDigit(c, NumericToken.Base.DECIMAL);

        private static bool IsDigit(char c, NumericToken.Base @base) =>
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

        /// <summary>
        /// Moves the cursor one step forward and returns the element which was previously current.
        /// </summary>
        /// <returns>The current element, before advancing the cursor.</returns>
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

        private void AddToken(Token token)
        {
            tokens.Add(token);
        }
    }
}
