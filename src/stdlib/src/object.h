#pragma once

#include <memory>

#include "perlang_string.h"
#include "ascii_string.h"

namespace perlang
{
    class Object
    {
     public:
        [[nodiscard]]
        std::unique_ptr<String> get_type() const
        {
            return ASCIIString::from_static_string("perlang::Object");
        }
    };
}
