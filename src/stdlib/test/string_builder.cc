// string_builder.cc - tests for the perlang::text::StringBuilder class

#include <catch2/catch_test_macros.hpp>

#include "perlang_stdlib.h"

TEST_CASE( "perlang::text::StringBuilder::append, resizing the string beyond its initial capacity" )
{
    // Arrange & Act
    perlang::text::StringBuilder sb;

    for (int i = 0; i < 100; i++) {
        sb.append(*perlang::ASCIIString::from_static_string("this is an ASCII string"));
    }

    uint expected_length = 100 * strlen("this is an ASCII string");

    // Assert
    REQUIRE(sb.length() == expected_length);
}
