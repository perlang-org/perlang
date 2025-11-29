#include <initializer_list>

#include "bigint.h"

#pragma once

namespace perlang
{
    // A class for representing mutable, fixed-size arrays of 32-bit unsigned integers.
    class UIntArray
    {
     public:
        // Creates a new UIntArray from a copied array of integers.
        explicit UIntArray(std::initializer_list<uint32_t> arr);

        // Creates a new UIntArray of the given size.
        explicit UIntArray(size_t length);

     private:
        UIntArray(uint32_t* arr, size_t length, bool owned);

     public:
        ~UIntArray();

        uint32_t operator[](size_t index) const;

        // C++ doesn't support operator overloading for assignment. We workaround this by just providing a set method
        // instead.
        void set(size_t index, uint32_t value);

        [[nodiscard]]
        bool contains(uint32_t value) const;

     private:
        uint32_t* arr_;
        size_t length_;
        bool owned_;
    };
}
