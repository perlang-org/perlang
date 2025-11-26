#include <initializer_list>

#include "bigint.h"

#pragma once

namespace perlang
{
    // A class for representing mutable, fixed-size arrays of boolean values.
    class BoolArray
    {
     public:
        // Creates a new BoolArray from a copied array of booleans.
        explicit BoolArray(std::initializer_list<bool> arr);

        // Creates a new BoolArray of the given size.
        explicit BoolArray(size_t length);

     private:
        BoolArray(bool* arr, size_t length, bool owned);

     public:
        ~BoolArray();

        bool operator[](size_t index) const;

        // C++ doesn't support operator overloading for assignment. We workaround this by just providing a set method
        // instead.
        void set(size_t index, bool value);

     private:
        bool* arr_;
        size_t length_;
        bool owned_;
    };
}
