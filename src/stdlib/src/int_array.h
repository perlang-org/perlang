#include <initializer_list>

#pragma once

namespace perlang
{
    // A class for representing mutable, fixed-size arrays of 32-bit integers.
    class IntArray
    {
     public:
        // Creates a new IntArray from a copied array of integers.
        explicit IntArray(std::initializer_list<int32_t> arr);

        // Creates a new IntArray of the given length.
        explicit IntArray(size_t length);

     private:
        IntArray(int32_t* arr, size_t length, bool owned);

     public:
        ~IntArray();

        int32_t operator[](size_t index) const;

        // C++ doesn't support operator overloading for assignment. We workaround this by just providing a set method
        // instead.
        void set(size_t index, int32_t value);

     private:
        int32_t* arr_;
        size_t length_;
        bool owned_;
    };
}
