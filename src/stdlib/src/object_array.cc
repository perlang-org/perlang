#include <array>
#include <stdexcept>
#include <string> // needed for std::to_string() on macOS/BSD

#include "object.h"
#include "object_array.h"

namespace perlang
{
    ObjectArray::ObjectArray(std::initializer_list<std::shared_ptr<const perlang::Object>> arr)
    {
        // Create a new buffer and copy the array into it. Note that this method performs a shallow copy by design.
        size_t length = arr.size();
        auto new_arr = new std::shared_ptr<const perlang::Object>[length];

        for (size_t i = 0; i < length; i++) {
            new_arr[i] = std::data(arr)[i];
        }

        arr_ = new_arr;
        length_ = length;
        owned_ = true;
    }

    ObjectArray::~ObjectArray()
    {
        if (owned_) {
            delete[] arr_;
        }
    }

    std::shared_ptr<const perlang::Object> ObjectArray::operator[](size_t index) const
    {
        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        return arr_[index];
    }

    size_t ObjectArray::length() const
    {
        return length_;
    }
}
