#include <memory.h>
#include <stdexcept>
#include <string>

#include "exceptions/null_pointer_exception.h"
#include "long_array.h"

namespace perlang
{
    LongArray::LongArray(std::initializer_list<int64_t> arr)
    {
        // Create a new buffer and copy the array into it.
        size_t length = arr.size();
        auto new_arr = (int64_t*)malloc(length * sizeof(int64_t));
        memcpy(new_arr, data(arr), length * sizeof(int64_t));

        arr_ = new_arr;
        length_ = length;
        owned_ = true;
    }

    LongArray::LongArray(size_t length)
    {
        arr_ = (int64_t*)calloc(length, sizeof(int64_t));
        length_ = length;
        owned_ = true;
    }

    LongArray::LongArray(int64_t* arr, size_t length, bool owned)
    {
        arr_ = arr;
        length_ = length;
        owned_ = owned;
    }

    LongArray::~LongArray()
    {
        if (owned_) {
            free((void*)arr_);
        }
    }

    int64_t LongArray::operator[](size_t index) const
    {
        if (this == nullptr) {
            throw NullPointerException();
        }

        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        return arr_[index];
    }

    void LongArray::set(size_t index, int64_t value)
    {
        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        arr_[index] = value;
    }
}
