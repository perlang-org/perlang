#pragma warning disable S4016

namespace Perlang
{
    public enum TokenType
    {
        // Single-character tokens.
        LEFT_PAREN,
        RIGHT_PAREN,
        LEFT_BRACE,
        RIGHT_BRACE,
        LEFT_SQUARE_BRACKET,
        RIGHT_SQUARE_BRACKET,
        COMMA,
        DOT,
        MINUS,
        PLUS,
        PERCENT,
        SINGLE_QUOTE,
        SEMICOLON,
        COLON,
        SLASH,
        AMPERSAND,
        QUESTION_MARK,
        CARET,
        VERTICAL_BAR,

        // One or two character tokens.
        BANG,
        BANG_EQUAL,
        EQUAL,
        EQUAL_EQUAL,
        GREATER,
        GREATER_EQUAL,
        GREATER_GREATER,
        LESS,
        LESS_EQUAL,
        LESS_LESS,
        PLUS_PLUS,
        MINUS_MINUS,
        PLUS_EQUAL,
        MINUS_EQUAL,
        STAR,
        STAR_STAR,

        // Literals.
        IDENTIFIER,
        STRING,
        NUMBER,

        // Keywords.
        AND,
        ELSE,
        FALSE,
        FUN,
        FOR,
        IF,
        NULL,
        OR,
        PRINT,
        RETURN,
        SUPER,
        THIS,
        TRUE,
        VAR,
        WHILE,

        // Reserved keywords, not yet used by the language.
        RESERVED_WORD,

        EOF
    }
}
