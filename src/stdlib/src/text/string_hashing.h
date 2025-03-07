#pragma once

#include <memory> // std::shared_ptr

#include "perlang_string.h"

class string_hasher
{
 public:
    int operator()(const std::shared_ptr<perlang::String>& x) const {
        // Based on an example from https://stackoverflow.com/a/2624210/227779
        int hash = 7;

        // TODO: Use x[i] instead, once we have an index operator for Perlang strings (and can avoid range checks for
        // TODO: every step in the loop)
        const char* bytes = x->bytes();

        for (size_t i = 0; i < x->length(); i++) {
            hash = hash * 31 + bytes[i];
        }

        return hash;
    }
};

// Needed because the default will compare the pointers I think (it doesn't work at least).
class string_comparer
{
 public:
    bool operator()(const std::shared_ptr<perlang::String>& x, const std::shared_ptr<perlang::String>& y) const {
        return *x == *y;
    }
};
