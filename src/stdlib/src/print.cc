#include <stdio.h>

// fmt is an open-source formatting library providing a fast and safe alternative to C stdio and C++ iostreams.
// https://github.com/fmtlib/fmt
#define FMT_HEADER_ONLY
#include "fmt/format.h"

#include "ascii_string.h"
#include "bigint.h"
#include "internal/string_utils.h"

namespace perlang
{
    void print(const String* str)
    {
        const char* bytes = str != nullptr ? str->bytes() : nullptr;

        // Safeguard against both `str` and `str->bytes()` potentially returning `null`
        if (bytes == nullptr) {
            puts("null");
        }
        else {
            // For C-style (NUL-terminated, char*) strings, there's no need to use the overhead which `printf` induces.
            // `puts` can potentially be a tiny bit faster.
            puts(bytes);
        }
    }

    void print(const ASCIIString& str)
    {
        print(&str);
    }

    void print(const std::unique_ptr<String>& str)
    {
        print(str.get());
    }

    void print(const std::unique_ptr<const String>& str)
    {
        print(str.get());
    }

    void print(const std::unique_ptr<ASCIIString>& str)
    {
        print(str.get());
    }

    void print(const std::unique_ptr<const ASCIIString>& str)
    {
        print(str.get());
    }

    void print(const std::unique_ptr<UTF8String>& str)
    {
        print(str.get());
    }

    void print(const std::unique_ptr<const UTF8String>& str)
    {
        print(str.get());
    }

    void print(const std::shared_ptr<const String>& str)
    {
        print(str.get());
    }

    void print(const std::shared_ptr<const ASCIIString>& str)
    {
        print(str.get());
    }

    void print(const std::shared_ptr<const UTF8String>& str)
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
        // The parameter type is defined slightly differently on different platforms, forcing us to do this
        // conditionally.
#ifdef __OpenBSD__
        printf("%lld\n", i);
#else
        printf("%ld\n", i);
#endif
    }

    void print(uint64_t u)
    {
        // The parameter type is defined slightly differently on different platforms, forcing us to do this
        // conditionally.
#ifdef __OpenBSD__
        printf("%llu\n", u);
#else
        printf("%lu\n", u);
#endif
    }

    void print(const BigInt& bigint)
    {
        puts(bigint.to_string().c_str());
    }

    void print(float f)
    {
        const std::string& str = internal::float_to_string(f) + "\n";
        fwrite(str.c_str(), str.length(), 1, stdout);
    }

    void print(double d)
    {
        const std::string& str = internal::double_to_string(d) + "\n";
        fwrite(str.c_str(), str.length(), 1, stdout);
    }
}
