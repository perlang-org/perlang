// print.cc - tests for the perlang::print() overloads

#include "src/stdlib.hpp"
#include "double-conversion//cctest.h"

bool fwrite_mocked = false;
char* captured_output = NULL;

extern "C" void __real_fwrite(const void *__restrict __ptr, size_t __size, size_t __n, FILE *__restrict __s);

extern "C" void __wrap_fwrite(const void *__restrict __ptr, size_t __size, size_t __n, FILE *__restrict __s)
{
    if (fwrite_mocked) {
        // TODO: handle multiple lines of output. Possibly append to previous list?
        if (captured_output != NULL) {
            free((void*)captured_output);
        }

        // __ptr is not necessarily NUL-terminated at this point, so we must be careful to only copy the data that would
        // have been printed to the file handle and then add a NUL terminator manually.
        unsigned long string_length = (__size * __n );
        captured_output = (char *)malloc(string_length + 1);
        strncpy(captured_output, (const char *)__ptr, string_length);
        captured_output[string_length] = '\0';
    }
    else {
        __real_fwrite(__ptr, __size, __n, __s);
    }
}

TEST(PrintFloat_103_1)
{
    fwrite_mocked = true;
    perlang::print(103.1f);
    fwrite_mocked = false;

    CHECK_EQ("103.1\n", captured_output);
}

TEST(PrintFloat_positive_infinity)
{
    float f = 1.0f / 0.0f;

    fwrite_mocked = true;
    perlang::print(f);
    fwrite_mocked = false;

    CHECK_EQ("Infinity\n", captured_output);
}

TEST(PrintDouble_123_45)
{
    fwrite_mocked = true;
    perlang::print(123.45);
    fwrite_mocked = false;

    CHECK_EQ("123.45\n", captured_output);
}

TEST(PrintDouble_minus_46_0)
{
    fwrite_mocked = true;
    perlang::print(-46.0);
    fwrite_mocked = false;

    CHECK_EQ("-46\n", captured_output);
}

TEST(PrintDouble_4294967296_123)
{
    fwrite_mocked = true;
    perlang::print(4294967296.123);
    fwrite_mocked = false;

    CHECK_EQ("4294967296.123\n", captured_output);
}

TEST(PrintDouble_4294967283)
{
    fwrite_mocked = true;
    perlang::print(4294967283.0);
    fwrite_mocked = false;

    CHECK_EQ("4294967283\n", captured_output);
}

TEST(PrintDouble_9223372036854775807)
{
    fwrite_mocked = true;
    perlang::print(9223372036854775807.0);
    fwrite_mocked = false;
}