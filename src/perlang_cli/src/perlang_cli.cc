// Automatically generated code by Perlang
// Do not modify. Changes to this file might be overwritten the next time the Perlang compiler is executed.

#include <locale.h> // setlocale()
#include <math.h> // fmod()
#include <memory> // std::shared_ptr
#include <stdint.h>

#include "perlang_stdlib.h"

#include "perlang_cli.h"

//
// Perlang class implementations
//
Token::Token(TokenType::TokenType token_type, std::shared_ptr<perlang::String> lexeme, std::shared_ptr<perlang::Object> literal, std::shared_ptr<perlang::String> file_name, int32_t line) {
    token_type_ = token_type;
    lexeme_ = lexeme;
    literal_ = literal;
    file_name_ = file_name;
    line_ = line;
};

TokenType::TokenType Token::type() {
    return token_type_;
};

std::shared_ptr<perlang::String> Token::lexeme() {
    return lexeme_;
};

std::shared_ptr<perlang::Object> Token::literal() {
    return literal_;
};

std::shared_ptr<perlang::String> Token::file_name() {
    return file_name_;
};

int32_t Token::line() {
    return line_;
};


//
// Perlang function declarations
//
std::shared_ptr<perlang::String> get_git_describe_version() {
    return perlang::ASCIIString::from_static_string("##GIT_DESCRIBE_VERSION##");
}

std::shared_ptr<perlang::String> get_git_commit_id() {
    return perlang::ASCIIString::from_static_string("##GIT_COMMIT_ID##");
}

std::shared_ptr<perlang::String> get_build_timestamp() {
    return perlang::ASCIIString::from_static_string("##BUILD_TIMESTAMP##");
}

std::shared_ptr<perlang::String> get_build_user() {
    return perlang::ASCIIString::from_static_string("##BUILD_USER##");
}

std::shared_ptr<perlang::String> get_build_host() {
    return perlang::ASCIIString::from_static_string("##BUILD_HOST##");
}

void print_perlang_version() {
    perlang::print(perlang_version());
}

std::shared_ptr<perlang::String> perlang_version() {
    return (*(*get_git_describe_version() + *perlang::ASCIIString::from_static_string("+")) + *get_git_commit_id());
}

void perlang_detailed_version() {
    perlang::print((*perlang::ASCIIString::from_static_string("Perlang version: ") + *perlang_version()));
    perlang::print((*(*(*(*(*(*(*perlang::ASCIIString::from_static_string("Built from git commit ") + *get_git_commit_id()) + *perlang::ASCIIString::from_static_string(", ")) + *get_build_timestamp()) + *perlang::ASCIIString::from_static_string(" by ")) + *get_build_user()) + *perlang::ASCIIString::from_static_string("@")) + *get_build_host()));
}

//
// C++ methods
//
extern "C" void native_main([[maybe_unused]] int argc, char* const* argv)
{
    // This is the entry point for the perlang CLI. It is currently C++-based and is called by the C# code. Because of
    // the parameter it takes, it cannot be replaced by pure Perlang for now.
    //
    // C++ code can quite easily call into Perlang code though, so what we can do is to call Perlang functions to
    // handle various options.

    // Disable warnings on unknown options in getopt_long(). This is a temporary remedy until we have converted all the
    // option parsing to C++/Perlang, at which point we should remove this to reenable those warnings. Until then, the
    // C# lib we use for option parsing will handle them anyway.
    opterr = 0;

    static struct option long_options[] = {
         { "version", no_argument,       nullptr,  'v' },
         { nullptr,   0,                 nullptr,  0   }
    };

    int* longindex = nullptr;
    int opt;
    while ((opt = getopt_long(argc, argv, "vV", long_options, longindex)) != -1) {
        switch (opt) {
            case 'v':
                print_perlang_version();
                exit(0);
                break;
            case 'V':
                perlang_detailed_version();
                exit(0);
                break;
            default:
                // Once we have the whole option parsing rewritten in C++/Perlang, we can enable this. Until then,
                // it will produce false positives about options that are handled on the C# side.
                //printf("?? getopt_long returned unexpected character code 0%o ??\n", opt);
                break;
        }
    }

    // Pass control back to the C# code
}
Token* create_string_token(TokenType::TokenType token_type, const char* lexeme, const char* literal, const char* file_name, int line)
{
    if (literal == nullptr) {
        throw std::invalid_argument("literal argument cannot be null");
    }

    return new Token(token_type, perlang::UTF8String::from_copied_string(lexeme), perlang::UTF8String::from_copied_string(literal), perlang::UTF8String::from_copied_string(file_name), line);
}

Token* create_char_token(TokenType::TokenType token_type, const char* lexeme, char16_t literal, const char* file_name, int line)
{
    return new Token(token_type, perlang::UTF8String::from_copied_string(lexeme), perlang::Char::from(literal), perlang::UTF8String::from_copied_string(file_name), line);
}

Token* create_null_token(TokenType::TokenType token_type, const char* lexeme, const char* file_name, int line)
{
    return new Token(token_type, perlang::UTF8String::from_copied_string(lexeme), nullptr, perlang::UTF8String::from_copied_string(file_name), line);
}

// Must be called explicitly from the C# side, since CppSharp doesn't give us an easy way to pass ownership over to C#.
void delete_token(Token* token)
{
    delete token;
}

bool is_string_token(Token* token)
{
    auto literal = token->literal().get();

    // TODO: Replace with proper is_assignable_to() check once we have a more sophisticated perlang::Type
    // implementation, supporting it.
    if (*literal->get_type() == *perlang::ASCIIString::from_static_string("perlang.ASCIIString") ||
        *literal->get_type() == *perlang::ASCIIString::from_static_string("perlang.UTF8String")) {
        return true;
    }
    else {
        return false;
    }
}

bool is_char_token(Token* token)
{
    auto literal = token->literal().get();

    return (*literal->get_type() == *perlang::ASCIIString::from_static_string("perlang.Char"));
}

const char* get_token_lexeme(Token* token)
{
    // TODO: This (and the other similar methods) work, under the crude assumption that the underlying string is
    // actually UTF-8 encoded (i.e. no UTF16String). When the 'as_utf8()' method exists in the String class, we should
    // use that to ensure that we don't get any surprises here.
    return token->lexeme()->bytes();
}

const char* get_token_string_literal(Token* token)
{
    auto literal = token->literal().get();

    // TODO: Replace with is_assignable_to() check
    if (*literal->get_type() == *perlang::ASCIIString::from_static_string("perlang.ASCIIString") ||
        *literal->get_type() == *perlang::ASCIIString::from_static_string("perlang.UTF8String")) {
        return ((perlang::String*)literal)->bytes();
    }
    else {
        throw perlang::IllegalStateException(*perlang::ASCIIString::from_static_string("Token expected to be string, not ") + *literal->get_type());
    }
}

uint16_t get_token_char_literal(Token* token)
{
    auto literal = token->literal().get();

    if (*literal->get_type() == *perlang::ASCIIString::from_static_string("perlang.Char")) {
        return ((perlang::Char*)literal)->value();
    }
    else {
        throw perlang::IllegalStateException(*perlang::ASCIIString::from_static_string("Token expected to be string, not ") + *literal->get_type());
    }
}

const char* get_token_file_name(Token* token)
{
    return token->file_name()->bytes();
}
