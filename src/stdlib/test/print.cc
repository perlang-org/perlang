// print.cc - tests for the perlang::print() overloads

#pragma clang diagnostic push
#pragma ide diagnostic ignored "bugprone-reserved-identifier"

#include <cstring>
#include <catch2/catch_test_macros.hpp>

#include "perlang_stdlib.h"

bool fwrite_mocked = false;
std::string* captured_output = nullptr;

extern "C" void __real_fwrite(const void *__restrict __ptr, size_t __size, size_t __n, FILE *__restrict __s);

extern "C" void __wrap_fwrite(const void *__restrict __ptr, size_t __size, size_t __n, FILE *__restrict __s)
{
    if (fwrite_mocked) {
        // TODO: handle multiple lines of output. Possibly append to previous list?
        if (captured_output != nullptr) {
            delete captured_output;
        }

        // __ptr is not necessarily NUL-terminated at this point, so we must be careful to only copy the data that would
        // have been printed to the file handle and then add a NUL terminator manually.
        unsigned long string_length = (__size * __n );
        char* captured_output_buffer = (char *)malloc(string_length + 1);
        strncpy(captured_output_buffer, (const char *)__ptr, string_length);
        captured_output_buffer[string_length] = '\0';

        captured_output = new std::string(captured_output_buffer);
    }
    else {
        __real_fwrite(__ptr, __size, __n, __s);
    }
}

TEST_CASE( "perlang::print float, 103.1f" )
{
    fwrite_mocked = true;
    perlang::print(103.1f);
    fwrite_mocked = false;

    REQUIRE(*captured_output == "103.1\n");
}

TEST_CASE( "perlang::print float, positive infinity" )
{
    float f = 1.0f / 0.0f;

    fwrite_mocked = true;
    perlang::print(f);
    fwrite_mocked = false;

    REQUIRE(*captured_output == "Infinity\n");
}

TEST_CASE( "perlang::print double, 123.45" )
{
    fwrite_mocked = true;
    perlang::print(123.45);
    fwrite_mocked = false;

    REQUIRE(*captured_output == "123.45\n");
}

TEST_CASE( "perlang::print double, -46.0" )
{
    fwrite_mocked = true;
    perlang::print(-46.0);
    fwrite_mocked = false;

    REQUIRE(*captured_output == "-46\n");
}

TEST_CASE( "perlang::print double, 4294967296.123" )
{
    fwrite_mocked = true;
    perlang::print(4294967296.123);
    fwrite_mocked = false;

    REQUIRE(*captured_output == "4294967296.123\n");
}

TEST_CASE( "perlang::print double, 4294967283" )
{
    fwrite_mocked = true;
    perlang::print(4294967283.0);
    fwrite_mocked = false;

    REQUIRE(*captured_output == "4294967283\n");
}

TEST_CASE( "perlang::print double, 9223372036854775807" )
{
    fwrite_mocked = true;
    perlang::print(9223372036854775807.0);
    fwrite_mocked = false;

    REQUIRE(*captured_output == "9.22337203685478E+18\n");
}

// TODO: Make this test work. We need to (linker-)wrap puts to make it happen.
TEST_CASE( "perlang::print BigInt, 18446744073709551616" )
{
//    // TODO: delete the above, keep this
//    fwrite_mocked = true;
//    perlang::print(BigInt("18446744073709551616"));
//    fwrite_mocked = false;
//
//    CHECK_EQ("18446744073709551616\n", captured_output);
}
