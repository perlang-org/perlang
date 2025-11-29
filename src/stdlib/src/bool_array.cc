#include <memory.h>
#include <stdexcept>
#include <string>

#include "exceptions/null_pointer_exception.h"
#include "bool_array.h"

namespace perlang
{
    BoolArray::BoolArray(std::initializer_list<bool> arr)
    {
        // Create a new buffer and copy the array into it.
        size_t length = arr.size();
        auto new_arr = (bool*)malloc(length * sizeof(bool));
        memcpy(new_arr, data(arr), length * sizeof(bool));

        arr_ = new_arr;
        length_ = length;
        owned_ = true;
    }

    BoolArray::BoolArray(size_t length)
    {
        arr_ = (bool*)calloc(length, sizeof(bool));
        length_ = length;
        owned_ = true;
    }

    BoolArray::BoolArray(bool* arr, size_t length, bool owned)
    {
        arr_ = arr;
        length_ = length;
        owned_ = owned;
    }

    BoolArray::~BoolArray()
    {
        if (owned_) {
            free((void*)arr_);
        }
    }

    bool BoolArray::operator[](size_t index) const
    {
        if (this == nullptr) {
            throw NullPointerException();
        }

        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        return arr_[index];
    }

    void BoolArray::set(size_t index, bool value)
    {
        // TODO: Add null check, but only if we can add a test that exercises the behavior.

        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        arr_[index] = value;
    }

    bool BoolArray::contains(bool value) const
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
