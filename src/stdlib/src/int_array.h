#pragma once

#include <memory>

namespace perlang
{
    // A class for representing mutable, fixed-size arrays of 32-bit integers.
    class IntArray
    {
     public:
        // Creates a new IntArray from a copied array of integers.
        IntArray(std::initializer_list<int32_t> arr);

     private:
        IntArray(const int32_t* arr, size_t length, bool owned);

     public:
        ~IntArray();

        int operator[](size_t index) const;

     private:
        const int32_t* arr_;
        size_t length_;
        bool owned_;
    };
}
