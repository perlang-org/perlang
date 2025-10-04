// string_token_type_dictionary.cc - tests for the StringTokenTypeDictionary class

#include <catch2/catch_test_macros.hpp>

#include "mutable_string_token_type_dictionary.h"

namespace Color {
    enum Color {
        RED,
        GREEN,
        BLUE,
    };
};

TEST_CASE( "MutableStringTokenTypeDictionary, contains_key returns true when an item has been added to the dictionary" )
{
    MutableStringTokenTypeDictionary dictionary;

    dictionary.add("some-key", TokenType::TokenType::STAR_STAR);

    // Assert
    REQUIRE(dictionary.get("some-key") == TokenType::TokenType::STAR_STAR);

    setlocale(LC_ALL, "");
    Color::Color a = Color::BLUE;
    Color::Color b = Color::GREEN;
    Color::Color c = Color::RED;
    perlang::print(a);
    perlang::print(b);
    perlang::print(c);
}
