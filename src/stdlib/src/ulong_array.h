#include <initializer_list>

#include "bigint.h"

#pragma once

namespace perlang
{
    // A class for representing mutable, fixed-size arrays of 64-bit unsigned integers.
    class ULongArray
    {
     public:
        // Creates a new ULongArray from a copied array of integers.
        explicit ULongArray(std::initializer_list<uint64_t> arr);

        // Creates a new ULongArray of the given size.
        explicit ULongArray(size_t length);

     private:
        ULongArray(uint64_t* arr, size_t length, bool owned);

     public:
        ~ULongArray();

        uint64_t operator[](size_t index) const;

        // C++ doesn't support operator overloading for assignment. We workaround this by just providing a set method
        // instead.
        void set(size_t index, uint64_t value);

        [[nodiscard]]
        bool contains(uint64_t value) const;

     private:
        uint64_t* arr_;
        size_t length_;
        bool owned_;
    };
}
