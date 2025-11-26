#include <initializer_list>

#include "bigint.h"

#pragma once

namespace perlang
{
    // A class for representing mutable, fixed-size arrays of 64-bit signed integers.
    class LongArray
    {
     public:
        // Creates a new LongArray from a copied array of long integers.
        explicit LongArray(std::initializer_list<int64_t> arr);

        // Creates a new LongArray of the given size.
        explicit LongArray(size_t length);

     private:
        LongArray(int64_t* arr, size_t length, bool owned);

     public:
        ~LongArray();

        int64_t operator[](size_t index) const;

        // C++ doesn't support operator overloading for assignment. We workaround this by just providing a set method
        // instead.
        void set(size_t index, int64_t value);

     private:
        int64_t* arr_;
        size_t length_;
        bool owned_;
    };
}
