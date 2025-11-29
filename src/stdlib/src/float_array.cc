#include <memory.h>
#include <stdexcept>
#include <string>

#include "exceptions/null_pointer_exception.h"
#include "float_array.h"

namespace perlang
{
    FloatArray::FloatArray(std::initializer_list<float> arr)
    {
        // Create a new buffer and copy the array into it.
        size_t length = arr.size();
        auto new_arr = (float*)malloc(length * sizeof(float));
        memcpy(new_arr, data(arr), length * sizeof(float));

        arr_ = new_arr;
        length_ = length;
        owned_ = true;
    }

    FloatArray::FloatArray(size_t length)
    {
        arr_ = (float*)calloc(length, sizeof(float));
        length_ = length;
        owned_ = true;
    }

    FloatArray::FloatArray(float* arr, size_t length, bool owned)
    {
        arr_ = arr;
        length_ = length;
        owned_ = owned;
    }

    FloatArray::~FloatArray()
    {
        if (owned_) {
            free((void*)arr_);
        }
    }

    float FloatArray::operator[](size_t index) const
    {
        if (this == nullptr) {
            throw NullPointerException();
        }

        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        return arr_[index];
    }

    void FloatArray::set(size_t index, float value)
    {
        // TODO: Add null check, but only if we can add a test that exercises the behavior.

        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        arr_[index] = value;
    }

    bool FloatArray::contains(float value) const
    {
        if (this == nullptr) {
            return false;
        }

        for (size_t i = 0; i < length_; i++) {
            if (arr_[i] == value) {
                return true;
            }
        }

        return false;
    }
}
