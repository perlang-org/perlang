#pragma once

#include <string>

// fmt is an open-source formatting library providing a fast and safe alternative to C stdio and C++ iostreams.
// https://github.com/fmtlib/fmt
#define FMT_HEADER_ONLY
#include "../fmt/format.h"

namespace perlang::internal
{
    inline std::string float_to_string(const float lhs)
    {
        // Use the same precision as in C#
        return fmt::format("{:.7G}", lhs);
    }

    inline std::string double_to_string(const double lhs)
    {
        // Use the same precision as in C#
        return fmt::format("{:.15G}", lhs);
    }
}
