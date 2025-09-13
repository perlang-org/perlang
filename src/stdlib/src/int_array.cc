#include <memory.h>
#include <stdexcept>
#include <string>

#include "exceptions/null_pointer_exception.h"
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

    IntArray::IntArray(size_t length)
    {
        arr_ = (int32_t*)calloc(length, sizeof(int32_t));
        length_ = length;
        owned_ = true;
    }

    int32_t IntArray::operator[](size_t index) const
    {
        if (this == nullptr) {
            throw NullPointerException();
        }

        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        return arr_[index];
    }

    void IntArray::set(size_t index, int32_t value)
    {
        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        arr_[index] = value;
    }

    IntArray::IntArray(int32_t* arr, size_t length, bool owned)
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
