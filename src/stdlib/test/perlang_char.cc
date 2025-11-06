// perlang_string.cc - tests for the perlang::String class

#include <catch2/catch_test_macros.hpp>

#include "perlang_stdlib.h"

TEST_CASE( "perlang::Char::to_upper, with ASCII content" )
{
    char16_t c = perlang::Char::to_upper('a');

    // Assert
    REQUIRE(c == 'A');
}

TEST_CASE( "perlang::Char::to_upper, with non-ASCII content" )
{
    REQUIRE(std::setlocale(LC_ALL, "sv_SE.UTF-8") != nullptr);
    char16_t c = perlang::Char::to_upper(L'å');

    // Assert
    REQUIRE(c == L'Å');
}

TEST_CASE( "perlang::Char::to_lower, with ASCII content" )
{
    char16_t c = perlang::Char::to_lower('Z');

    // Assert
    REQUIRE(c == 'z');
}

TEST_CASE( "perlang::Char::to_lower, with non-ASCII content" )
{
    REQUIRE(std::setlocale(LC_ALL, "sv_SE.UTF-8") != nullptr);
    char16_t c = perlang::Char::to_lower(L'Å');

    // Assert
    REQUIRE(c == L'å');
}
