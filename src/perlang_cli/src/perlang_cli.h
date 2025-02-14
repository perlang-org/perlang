// Automatically generated code by Perlang
// Do not modify. Changes to this file might be overwritten the next time the Perlang compiler is executed.

//
// C++ prototypes
//
#include <getopt.h>

extern "C" void native_main(int argc, char* const* argv);

//
// Enum definitions
//
namespace Visibility {
    enum Visibility {
        Public,
    };
};

namespace TokenType {
    enum TokenType {
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
        IDENTIFIER,
        STRING,
        NUMBER,
        PREPROCESSOR_DIRECTIVE_CPP_PROTOTYPES,
        PREPROCESSOR_DIRECTIVE_CPP_METHODS,
        CLASS,
        CONSTRUCTOR,
        DESTRUCTOR,
        ELSE,
        ENUM,
        FALSE,
        FUN,
        FOR,
        IF,
        NEW,
        PERLANG_NULL,
        PRINT,
        PUBLIC,
        RETURN,
        SUPER,
        THIS,
        TRUE,
        VAR,
        WHILE,
        RESERVED_WORD,
        PERLANG_EOF,
    };
};

//
// Method definitions
//
std::shared_ptr<perlang::String> get_git_describe_version();
std::shared_ptr<perlang::String> get_git_commit();
std::shared_ptr<perlang::String> get_build_timestamp();
std::shared_ptr<perlang::String> get_build_user();
std::shared_ptr<perlang::String> get_build_host();
void perlang_version();
void perlang_detailed_version();
