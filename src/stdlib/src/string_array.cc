#include <memory>
#include <stdexcept>

#include "ascii_string.h"
#include "string_array.h"

namespace perlang
{
    StringArray::StringArray(std::initializer_list<std::shared_ptr<const perlang::String>> arr)
    {
        // Create a new buffer and copy the array into it.
        size_t length = arr.size();
        auto new_arr = new std::shared_ptr<const perlang::String>[length];

        for (size_t i = 0; i < length; i++) {
            new_arr[i] = std::data(arr)[i];
        }

        arr_ = new_arr;
        length_ = length;
        owned_ = true;
    }

    StringArray::~StringArray()
    {
        if (owned_) {
            delete[] arr_;
        }
    }

    std::shared_ptr<const perlang::String> StringArray::operator[](size_t index) const
    {
        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        return arr_[index];
    }
}
