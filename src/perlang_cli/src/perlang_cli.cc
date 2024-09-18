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
// Method definitions
//
std::shared_ptr<const perlang::String> get_git_tag_version();
std::shared_ptr<const perlang::String> get_git_describe_version();
std::shared_ptr<const perlang::String> get_git_commit();
void perlang_version();

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
    struct option* longopts = nullptr;
    int* longindex = nullptr;
    int opt;
    while ((opt = getopt_long(argc, argv, "v", longopts, longindex)) != -1) {
        switch (opt) {
            case 'v':
                perlang_version();
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
std::shared_ptr<const perlang::String> get_git_tag_version() {
    return perlang::ASCIIString::from_static_string("0.6.0");
}

std::shared_ptr<const perlang::String> get_git_describe_version() {
    return perlang::ASCIIString::from_static_string("0.6.0-dev.22");
}

std::shared_ptr<const perlang::String> get_git_commit() {
    return perlang::ASCIIString::from_static_string("feba4bf");
}

void perlang_version() {
    perlang::print(get_git_describe_version());
}

