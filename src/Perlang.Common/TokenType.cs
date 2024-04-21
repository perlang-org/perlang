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
        QUESTION_MARK,
        CARET,

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
        AMPERSAND,
        AMPERSAND_AMPERSAND,
        PIPE,
        PIPE_PIPE,
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

        // #-directives, used for enabling special features in the compiler. These are not part of the language itself,
        // and each directive has a token type of its own.
        PREPROCESSOR_DIRECTIVE_CPP_PROTOTYPES,
        PREPROCESSOR_DIRECTIVE_CPP_METHODS,

        // Keywords.
        ELSE,
        FALSE,
        FUN,
        FOR,
        IF,
        NULL,
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
