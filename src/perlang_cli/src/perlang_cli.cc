// Automatically generated code by Perlang
// Do not modify. Changes to this file might be overwritten the next time the Perlang compiler is executed.

#include <math.h> // fmod()
#include <memory> // std::shared_ptr
#include <stdint.h>

#include "perlang_stdlib.h"
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

//
// C++ methods
//
extern "C" void native_main([[maybe_unused]] int argc, char* const* argv)
{
    // This is the entry pint for the perlang CLI. It is currently C++-based and is called by the C# code. Because of
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
                perlang_version();
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

//
// Method declarations
//
std::shared_ptr<perlang::String> get_git_describe_version() {
    return perlang::ASCIIString::from_static_string("##GIT_DESCRIBE_VERSION##");
}

std::shared_ptr<perlang::String> get_git_commit() {
    return perlang::ASCIIString::from_static_string("##GIT_COMMIT##");
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

void perlang_version() {
    perlang::print(get_git_describe_version());
}

void perlang_detailed_version() {
    perlang::print((*perlang::ASCIIString::from_static_string("Perlang version: ") + *get_git_describe_version()));
    perlang::print((*(*(*(*(*(*(*perlang::ASCIIString::from_static_string("Built from git commit ") + *get_git_commit()) + *perlang::ASCIIString::from_static_string(", ")) + *get_build_timestamp()) + *perlang::ASCIIString::from_static_string(" by ")) + *get_build_user()) + *perlang::ASCIIString::from_static_string("@")) + *get_build_host()));
}

