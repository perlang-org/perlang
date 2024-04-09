#include <stdio.h>

// fmt is an open-source formatting library providing a fast and safe alternative to C stdio and C++ iostreams.
// https://github.com/fmtlib/fmt
#define FMT_HEADER_ONLY
#include "fmt/format.h"

#include "ascii_string.h"
#include "bigint.hpp"

namespace perlang
{
    void print(const String* str)
    {
        // Safeguard against both `str` and `str->bytes()` potentially returning `null`
        const char* bytes = str != nullptr ? str->bytes() : nullptr;

        if (bytes == nullptr) {
            puts("null");
        }
        else {
            // For plain strings, there's no need to use the overhead which `printf` induces. `puts` can potentially be a
            // tiny bit faster.
            puts(bytes);
        }
    }

    void print(const ASCIIString& str)
    {
        print(&str);
    }

    void print(const std::shared_ptr<const String>& str)
    {
        print(str.get());
    }

    void print(const std::shared_ptr<const ASCIIString>& str)
    {
        print(str.get());
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

    void print(const BigInt& bigint)
    {
        puts(bigint.to_string().c_str());
    }

    void print(float f)
    {
        // Use the same precision as on the C# side
        fmt::println("{:.7G}", f);
    }

    void print(double d)
    {
        // Use the same precision as on the C# side
        fmt::println("{:.15G}", d);
    }
}
