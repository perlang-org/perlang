// int_array_tests.cc - tests for the perlang::IntArray class

#include <catch2/catch_test_macros.hpp>

#include "perlang_stdlib.h"

TEST_CASE( "perlang::IntArray, throws expected error when null IntArray is indexed" )
{
    std::shared_ptr<perlang::IntArray> a;

    // Assert
    //
    // This would normally cause a SIGSEGV, but our index operator tries hard to prevent it.
    REQUIRE_THROWS_AS((*a)[0], perlang::NullPointerException);
}
