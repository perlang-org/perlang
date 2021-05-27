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
        COMMA,
        DOT,
        MINUS,
        PLUS,
        PERCENT,
        SEMICOLON,
        COLON,
        SLASH,

        // One or two character tokens.
        BANG,
        BANG_EQUAL,
        EQUAL,
        EQUAL_EQUAL,
        GREATER,
        GREATER_EQUAL,
        LESS,
        LESS_EQUAL,
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
