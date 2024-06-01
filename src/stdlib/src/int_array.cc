#include <memory.h>
#include <stdexcept>

#include "int_array.h"

namespace perlang
{
    IntArray::IntArray(std::initializer_list<int32_t> arr)
    {
        // Create a new buffer and copy the array into it.
        size_t length = arr.size();
        auto new_arr = (int32_t*)malloc(length * sizeof(int32_t));
        memcpy(new_arr, data(arr), length * sizeof(int32_t));

        arr_ = new_arr;
        length_ = length;
        owned_ = true;
    }

    int IntArray::operator[](size_t index) const
    {
        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        return arr_[index];
    }

    IntArray::IntArray(const int32_t* arr, size_t length, bool owned)
    {
        arr_ = arr;
        length_ = length;
        owned_ = owned;
    }

    IntArray::~IntArray()
    {
        if (owned_) {
            free((void*)arr_);
        }
    }
}
