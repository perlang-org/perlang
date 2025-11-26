#include <initializer_list>

#include "bigint.h"

#pragma once

namespace perlang
{
    // A class for representing mutable, fixed-size arrays of UTF-16LE characters.
    class CharArray
    {
     public:
        // Creates a new CharArray from a copied array of characters.
        explicit CharArray(std::initializer_list<char16_t> arr);

        // Creates a new CharArray of the given size.
        explicit CharArray(size_t length);

     private:
        CharArray(char16_t* arr, size_t length, bool owned);

     public:
        ~CharArray();

        char16_t operator[](size_t index) const;

        // C++ doesn't support operator overloading for assignment. We workaround this by just providing a set method
        // instead.
        void set(size_t index, char16_t value);

     private:
        char16_t* arr_;
        size_t length_;
        bool owned_;
    };
}
