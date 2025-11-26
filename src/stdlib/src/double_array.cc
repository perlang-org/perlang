#include <memory.h>
#include <stdexcept>
#include <string>

#include "exceptions/null_pointer_exception.h"
#include "double_array.h"

namespace perlang
{
    DoubleArray::DoubleArray(std::initializer_list<double> arr)
    {
        // Create a new buffer and copy the array into it.
        size_t length = arr.size();
        auto new_arr = (double*)malloc(length * sizeof(double));
        memcpy(new_arr, data(arr), length * sizeof(double));

        arr_ = new_arr;
        length_ = length;
        owned_ = true;
    }

    DoubleArray::DoubleArray(size_t length)
    {
        arr_ = (double*)calloc(length, sizeof(double));
        length_ = length;
        owned_ = true;
    }

    DoubleArray::DoubleArray(double* arr, size_t length, bool owned)
    {
        arr_ = arr;
        length_ = length;
        owned_ = owned;
    }

    DoubleArray::~DoubleArray()
    {
        if (owned_) {
            free((void*)arr_);
        }
    }

    double DoubleArray::operator[](size_t index) const
    {
        if (this == nullptr) {
            throw NullPointerException();
        }

        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        return arr_[index];
    }

    void DoubleArray::set(size_t index, double value)
    {
        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        arr_[index] = value;
    }
}
