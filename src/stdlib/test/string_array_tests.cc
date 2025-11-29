// string_array_tests.cc - tests for the perlang::StringArray class

#include <catch2/catch_test_macros.hpp>

#include "perlang_stdlib.h"

TEST_CASE("perlang::StringArray::contains returns true with ASCII string present in the array")
{
    std::shared_ptr<perlang::StringArray> a = std::make_shared<perlang::StringArray>(std::initializer_list<std::shared_ptr<const perlang::String>>{
        perlang::ASCIIString::from_static_string("one"),
        perlang::ASCIIString::from_static_string("two"),
        perlang::ASCIIString::from_static_string("three")
    });

    REQUIRE(a->contains(perlang::ASCIIString::from_static_string("two")));
}
