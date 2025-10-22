// Automatically generated code by Perlang
// Do not modify. Changes to this file might be overwritten the next time the Perlang compiler is executed.

#pragma once

#include <memory> // std::shared_ptr
#include <stdint.h>

#include "perlang_stdlib.h"

//
// Perlang enum definitions
//
namespace Visibility {
    enum Visibility {
        Unspecified,
        Public,
        Private,
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
        CHAR,
        IDENTIFIER,
        STRING,
        NUMBER,
        PREPROCESSOR_DIRECTIVE_CPP_PROTOTYPES,
        PREPROCESSOR_DIRECTIVE_CPP_METHODS,
        CLASS,
        CONSTRUCTOR,
        DESTRUCTOR,
        ELSE,
        EXTERN,
        ENUM,
        FALSE,
        FUN,
        FOR,
        IF,
        MUTABLE,
        NEW,
        PERLANG_NULL,
        PRINT,
        PRIVATE,
        PUBLIC,
        RETURN,
        STATIC,
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
// Perlang class definitions
//
class Token : public std::enable_shared_from_this<Token>, public perlang::Object {
private:
    TokenType::TokenType token_type_;
    std::shared_ptr<perlang::String> lexeme_;
    std::shared_ptr<perlang::Object> literal_;
    std::shared_ptr<perlang::String> file_name_;
    int32_t line_;
public:
    Token(TokenType::TokenType token_type, std::shared_ptr<perlang::String> lexeme, std::shared_ptr<perlang::Object> literal, std::shared_ptr<perlang::String> file_name, int32_t line);
    TokenType::TokenType type();
    std::shared_ptr<perlang::String> lexeme();
    std::shared_ptr<perlang::Object> literal();
    std::shared_ptr<perlang::String> file_name();
    int32_t line();
private:
};

//
// Perlang function definitions
//
std::shared_ptr<perlang::String> get_git_describe_version();
std::shared_ptr<perlang::String> get_git_commit_id();
std::shared_ptr<perlang::String> get_build_timestamp();
std::shared_ptr<perlang::String> get_build_user();
std::shared_ptr<perlang::String> get_build_host();
void print_perlang_version();
std::shared_ptr<perlang::String> perlang_version();
void perlang_detailed_version();

//
// C++ prototypes
//
#include <getopt.h>

extern "C" void native_main(int argc, char* const* argv);
Token* create_string_token(TokenType::TokenType token_type, const char* lexeme, const char* literal, const char* file_name, int line);
Token* create_char_token(TokenType::TokenType token_type, const char* lexeme, char16_t literal, const char* file_name, int line);
Token* create_null_token(TokenType::TokenType token_type, const char* lexeme, const char* file_name, int line);
void delete_token(Token* token);

bool is_string_token(Token* token);
bool is_char_token(Token* token);

const char* get_token_lexeme(Token* token);
const char* get_token_string_literal(Token* token);
uint16_t get_token_char_literal(Token* token);
const char* get_token_file_name(Token* token);
