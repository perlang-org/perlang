// perlang_string.cc - tests for the perlang::String class

#include <catch2/catch_test_macros.hpp>

#include "perlang_stdlib.h"

TEST_CASE( "perlang::String, comparing with String" )
{
    std::shared_ptr<const perlang::String> s1 = perlang::ASCIIString::from_static_string("this is a string");
    std::shared_ptr<const perlang::String> s2 = perlang::ASCIIString::from_static_string("this is a string");

    // Assert
    REQUIRE(*s1 == *s2);
}

TEST_CASE( "perlang::String, comparing ASCIIString with unequal UTF8String" )
{
    std::shared_ptr<const perlang::String> s1 = perlang::ASCIIString::from_static_string("this is a string");
    std::shared_ptr<const perlang::String> s2 = perlang::UTF8String::from_static_string("this is a string with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし");

    // Assert
    REQUIRE(*s1 != *s2);
}
