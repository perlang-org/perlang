#pragma once

#include <memory>
#include <stdexcept>

#include "perlang_string.h"

namespace perlang
{
    class NullPointerException : public std::runtime_error
    {
        public:
            NullPointerException() :
                std::runtime_error("NullPointerException: Attempting to dereference null pointer") {
            }
    };
}
