#include <initializer_list>

#include "bigint.h"

#pragma once

namespace perlang
{
    // A class for representing mutable, fixed-size arrays of double-precision (64-bit) floating point values.
    class DoubleArray
    {
     public:
        // Creates a new DoubleArray from a copied array of doubles.
        explicit DoubleArray(std::initializer_list<double> arr);

        // Creates a new DoubleArray of the given size.
        explicit DoubleArray(size_t length);

     private:
        DoubleArray(double* arr, size_t length, bool owned);

     public:
        ~DoubleArray();

        double operator[](size_t index) const;

        // C++ doesn't support operator overloading for assignment. We workaround this by just providing a set method
        // instead.
        void set(size_t index, double value);

     private:
        double* arr_;
        size_t length_;
        bool owned_;
    };
}
