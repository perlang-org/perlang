// string_token_type_dictionary.cc - tests for the StringTokenTypeDictionary class

#include <catch2/catch_test_macros.hpp>

#include "mutable_string_token_type_dictionary.h"

TEST_CASE( "MutableStringTokenTypeDictionary, contains_key returns true when an item has been added to the dictionary" )
{
    MutableStringTokenTypeDictionary dictionary;

    dictionary.add("some-key", TokenType::TokenType::STAR_STAR);

    // Assert
    REQUIRE(dictionary.get("some-key") == TokenType::TokenType::STAR_STAR);
}
