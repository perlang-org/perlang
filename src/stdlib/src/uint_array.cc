#include <memory.h>
#include <stdexcept>
#include <string>

#include "exceptions/null_pointer_exception.h"
#include "uint_array.h"

namespace perlang
{
    UIntArray::UIntArray(std::initializer_list<uint32_t> arr)
    {
        // Create a new buffer and copy the array into it.
        size_t length = arr.size();
        auto new_arr = (uint32_t*)malloc(length * sizeof(uint32_t));
        memcpy(new_arr, data(arr), length * sizeof(uint32_t));

        arr_ = new_arr;
        length_ = length;
        owned_ = true;
    }

    UIntArray::UIntArray(size_t length)
    {
        arr_ = (uint32_t*)calloc(length, sizeof(uint32_t));
        length_ = length;
        owned_ = true;
    }

    UIntArray::UIntArray(uint32_t* arr, size_t length, bool owned)
    {
        arr_ = arr;
        length_ = length;
        owned_ = owned;
    }

    UIntArray::~UIntArray()
    {
        if (owned_) {
            free((void*)arr_);
        }
    }

    uint32_t UIntArray::operator[](size_t index) const
    {
        if (this == nullptr) {
            throw NullPointerException();
        }

        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        return arr_[index];
    }

    void UIntArray::set(size_t index, uint32_t value)
    {
        // TODO: Add null check, but only if we can add a test that exercises the behavior.

        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        arr_[index] = value;
    }

    bool UIntArray::contains(uint32_t value) const
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
