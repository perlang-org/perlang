// utf16_string.cc - tests for the perlang::UTF16String class

#include <codecvt>
#include <locale>
#include <memory>
#include <catch2/catch_test_macros.hpp>

#include "perlang_stdlib.h"

TEST_CASE( "perlang::UTF16String::from_copied_string(), returns an identical string for ASCII-only string" )
{
    std::string original_utf8 = "this is a an ASCII string";
    std::unique_ptr<perlang::UTF16String> s = perlang::UTF16String::from_copied_string(original_utf8.c_str());

    std::wstring_convert<std::codecvt_utf8_utf16<char16_t>, char16_t> convert;
    std::string utf8 = convert.to_bytes((char16_t *)s->bytes());

    REQUIRE(original_utf8 == utf8);
}

TEST_CASE( "perlang::UTF16String::from_copied_string(), returns an identical string for non-ASCII string" )
{
    std::string original_utf8 = "this is a string with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし🎉";
    std::unique_ptr<perlang::UTF16String> s = perlang::UTF16String::from_copied_string(original_utf8.c_str());

    std::wstring_convert<std::codecvt_utf8_utf16<char16_t>, char16_t> convert;
    std::string utf8 = convert.to_bytes((char16_t *)s->bytes());

    REQUIRE(original_utf8 == utf8);
}

TEST_CASE( "perlang::UTF16String::is_ascii(), returns true for ASCII-only string" )
{
    std::unique_ptr<perlang::UTF16String> s = perlang::UTF16String::from_copied_string("this is a an ASCII string");

    // Assert
    REQUIRE(s->is_ascii());
}

TEST_CASE( "perlang::UTF16String::is_ascii(), returns true for non-ASCII string" )
{
    std::unique_ptr<perlang::UTF16String> s = perlang::UTF16String::from_copied_string("this is a string with non-ASCII characters: åäöÅÄÖéèüÜÿŸïÏすし🎉");

    // Assert
    REQUIRE_FALSE(s->is_ascii());
}
