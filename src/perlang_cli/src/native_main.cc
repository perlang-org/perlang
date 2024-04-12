// Automatically generated code by Perlang
// Do not modify. Changes to this file might be overwritten the next time the Perlang compiler is executed.

#include <math.h> // fmod()
#include <memory> // std::shared_ptr
#include <stdint.h>

#include "perlang_stdlib.h"
//
// C++ prototypes
//
extern "C" int native_main(int argc, const char** argv);

//
// Method definitions
//
int32_t perlang_main(std::shared_ptr<const perlang::String> arg1);

//
// C++ methods
//
extern "C" int native_main([[maybe_unused]] int argc, const char **argv)
{
    // TODO: range check
    return perlang_main(perlang::ASCIIString::from_copied_string(argv[0]));
}

//
// Method declarations
//
int32_t perlang_main(std::shared_ptr<const perlang::String> arg1) {
    perlang::print((*perlang::ASCIIString::from_static_string("Hello World from *Native* *Code*! First parameter is: ") + *arg1));
    return 0;
}

