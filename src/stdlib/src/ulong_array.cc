#include <memory.h>
#include <stdexcept>
#include <string>

#include "exceptions/null_pointer_exception.h"
#include "ulong_array.h"

namespace perlang
{
    ULongArray::ULongArray(std::initializer_list<uint64_t> arr)
    {
        // Create a new buffer and copy the array into it.
        size_t length = arr.size();
        auto new_arr = (uint64_t*)malloc(length * sizeof(uint64_t));
        memcpy(new_arr, data(arr), length * sizeof(uint64_t));

        arr_ = new_arr;
        length_ = length;
        owned_ = true;
    }

    ULongArray::ULongArray(size_t length)
    {
        arr_ = (uint64_t*)calloc(length, sizeof(uint64_t));
        length_ = length;
        owned_ = true;
    }

    ULongArray::ULongArray(uint64_t* arr, size_t length, bool owned)
    {
        arr_ = arr;
        length_ = length;
        owned_ = owned;
    }

    ULongArray::~ULongArray()
    {
        if (owned_) {
            free((void*)arr_);
        }
    }

    uint64_t ULongArray::operator[](size_t index) const
    {
        if (this == nullptr) {
            throw NullPointerException();
        }

        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        return arr_[index];
    }

    void ULongArray::set(size_t index, uint64_t value)
    {
        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        arr_[index] = value;
    }
}
