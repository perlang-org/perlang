#include <initializer_list>

#include "bigint.h"

#pragma once

namespace perlang
{
    // A class for representing mutable, fixed-size arrays of big (larger than 64-bit) integers.
    class BigIntArray
    {
     public:
        // Creates a new BigIntArray from a copied array of bit integers.
        explicit BigIntArray(std::initializer_list<BigInt> arr);

        // Creates a new BigIntArray of the given size.
        explicit BigIntArray(size_t length);

     private:
        BigIntArray(BigInt* arr, size_t length, bool owned);

     public:
        ~BigIntArray();

        BigInt operator[](size_t index) const;

        // C++ doesn't support operator overloading for assignment. We workaround this by just providing a set method
        // instead.
        void set(size_t index, const BigInt& value);

        [[nodiscard]]
        bool contains(const BigInt& value) const;

     private:
        BigInt* arr_;
        size_t length_;
        bool owned_;
    };
}
