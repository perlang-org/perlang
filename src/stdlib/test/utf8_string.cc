// utf8_string.cc - tests for the perlang::UTF8String

#include <catch2/catch_test_macros.hpp>
#include <memory>

#include "perlang_stdlib.h"

TEST_CASE( "perlang::UTF8String::is_ascii(), returns true for ASCII-only string" )
{
    std::shared_ptr<perlang::UTF8String> s = perlang::UTF8String::from_static_string("this is a an ASCII string");

    // Assert
    REQUIRE(s->is_ascii());
}

TEST_CASE( "perlang::UTF8String::is_ascii(), returns true for non-ASCII string" )
{
    std::shared_ptr<perlang::UTF8String> s = perlang::UTF8String::from_static_string("this is a string with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし");

    // Assert
    REQUIRE_FALSE(s->is_ascii());
}
