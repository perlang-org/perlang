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

TEST_CASE( "perlang::text::StringBuilder::append, one character at a time up to 3000 in total length" )
{
    // Arrange & Act
    perlang::text::StringBuilder sb;

    for (int i = 0; i < 3000; i++) {
        sb.append(*perlang::ASCIIString::from_static_string("a"));
    }

    // Assert
    REQUIRE(sb.length() == 3000);
}

// The initial capacity is 1024 characters. We previously had a bug that would make the append() method crash if the
// first string added was > twice the initial capacity (because only the current capacity would be taken into account
// when expanding the buffer, not the size of the string being added)
TEST_CASE( "perlang::text::StringBuilder::append, a string longer than 2048 characters" )
{
    // Arrange & Act
    perlang::text::StringBuilder sb1;

    for (int i = 0; i < 2500; i++) {
        sb1.append(*perlang::ASCIIString::from_static_string("a"));
    }

    auto s = sb1.to_string();

    perlang::text::StringBuilder sb2;
    sb2.append(*s);

    // Assert
    REQUIRE(sb2.length() == 2500);
}

TEST_CASE( "perlang::text::StringBuilder::append, ASCII string" )
{
    // Arrange & Act
    perlang::text::StringBuilder sb;
    sb.append(*perlang::ASCIIString::from_static_string("this is an ASCII string"));

    // Assert
    REQUIRE(*sb.to_string() == *perlang::UTF8String::from_static_string("this is an ASCII string"));
}

TEST_CASE( "perlang::text::StringBuilder::append, UTF8 string" )
{
    // Arrange & Act
    perlang::text::StringBuilder sb;
    sb.append(*perlang::UTF8String::from_static_string("this is a UTF8 string: åäöÅÄÖéèüÜÿŸïÏすし"));

    // Assert
    REQUIRE(*sb.to_string() == *perlang::UTF8String::from_static_string("this is a UTF8 string: åäöÅÄÖéèüÜÿŸïÏすし"));
}
