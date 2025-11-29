#pragma once

#include <memory>
#include <initializer_list>

#include "perlang_string.h"

namespace perlang
{
    // A class for representing mutable, fixed-size arrays of perlang::String instances
    class StringArray
    {
     public:
        // Creates a new StringArray from a copied array of strings. Note that the strings themselves are not copied.
        // Strings are immutable in Perlang so reusing a string is safe; the shared_ptr will ensure that the string gets
        // deallocated when it's no longer needed.
        StringArray(std::initializer_list<std::shared_ptr<const perlang::String>> arr);

        // TODO: Support array constructor with fixed size (see IntArrayTests for relevant tests that can be copied)

        ~StringArray();

        std::shared_ptr<const perlang::String> operator[](size_t index) const;

        // The length of the array.
        [[nodiscard]]
        size_t length() const;

        [[nodiscard]]
        bool contains(std::shared_ptr<perlang::String> value) const;

     private:
        std::shared_ptr<const perlang::String>* arr_;
        size_t length_;
        bool owned_;
    };
}
