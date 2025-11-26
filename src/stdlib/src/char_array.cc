#include <memory.h>
#include <stdexcept>
#include <string>

#include "exceptions/null_pointer_exception.h"
#include "char_array.h"

namespace perlang
{
    CharArray::CharArray(std::initializer_list<char16_t> arr)
    {
        // Create a new buffer and copy the array into it.
        size_t length = arr.size();
        auto new_arr = (char16_t*)malloc(length * sizeof(char16_t));
        memcpy(new_arr, data(arr), length * sizeof(char16_t));

        arr_ = new_arr;
        length_ = length;
        owned_ = true;
    }

    CharArray::CharArray(size_t length)
    {
        arr_ = (char16_t*)calloc(length, sizeof(char16_t));
        length_ = length;
        owned_ = true;
    }

    CharArray::CharArray(char16_t* arr, size_t length, bool owned)
    {
        arr_ = arr;
        length_ = length;
        owned_ = owned;
    }

    CharArray::~CharArray()
    {
        if (owned_) {
            free((void*)arr_);
        }
    }

    char16_t CharArray::operator[](size_t index) const
    {
        if (this == nullptr) {
            throw NullPointerException();
        }

        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        return arr_[index];
    }

    void CharArray::set(size_t index, char16_t value)
    {
        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        arr_[index] = value;
    }
}
