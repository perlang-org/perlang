#include <initializer_list>

#include "bigint.h"

#pragma once

namespace perlang
{
    // A class for representing mutable, fixed-size arrays of single-precision (32-bit) floating point values.
    class FloatArray
    {
     public:
        // Creates a new FloatArray from a copied array of floats.
        explicit FloatArray(std::initializer_list<float> arr);

        // Creates a new FloatArray of the given size.
        explicit FloatArray(size_t length);

     private:
        FloatArray(float* arr, size_t length, bool owned);

     public:
        ~FloatArray();

        float operator[](size_t index) const;

        // C++ doesn't support operator overloading for assignment. We workaround this by just providing a set method
        // instead.
        void set(size_t index, float value);

        [[nodiscard]]
        bool contains(float value) const;

     private:
        float* arr_;
        size_t length_;
        bool owned_;
    };
}
