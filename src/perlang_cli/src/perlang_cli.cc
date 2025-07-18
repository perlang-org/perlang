// Automatically generated code by Perlang
// Do not modify. Changes to this file might be overwritten the next time the Perlang compiler is executed.

#include <locale.h> // setlocale()
#include <math.h> // fmod()
#include <memory> // std::shared_ptr
#include <stdint.h>

#include "perlang_stdlib.h"

#include "perlang_cli.h"

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

//
// Method declarations
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

