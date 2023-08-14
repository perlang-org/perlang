#include <stdint.h>
#include <stdio.h>

// fmt is an open-source formatting library providing a fast and safe alternative to C stdio and C++ iostreams.
// https://github.com/fmtlib/fmt
#define FMT_HEADER_ONLY
#include "fmt/format.h"

namespace perlang
{
    void print(const char* str)
    {
        // For plain strings, there's no need to use the overhead which `printf` induces. `puts` can potentially be a
        // tiny bit faster.
        puts(str);
    }

    void print(bool b)
    {
        if (b) {
            puts("true");
        }
        else {
            puts("false");
        }
    }

    void print(char c)
    {
        printf("%c\n", c);
    }

    void print(int32_t i)
    {
        printf("%d\n", i);
    }

    void print(uint32_t u)
    {
        printf("%u\n", u);
    }

    void print(int64_t i)
    {
        printf("%lld\n", i);
    }

    void print(uint64_t u)
    {
        printf("%llu\n", u);
    }

    void print(float f)
    {
        fmt::println("{}", f);
    }

    void print(double d)
    {
        fmt::println("{}", d);
    }
}
