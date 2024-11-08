// ascii_string.cc - tests for the perlang::ASCIIString class

#include <catch2/catch_test_macros.hpp>

#include "perlang_stdlib.h"

TEST_CASE( "perlang::ASCIIString, throws expected error when initialized with non-ASCII content" )
{
    // Assert
    REQUIRE_THROWS_AS(perlang::ASCIIString::from_static_string("this is a string with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし"), std::exception);
}
