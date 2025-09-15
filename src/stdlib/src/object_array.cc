#include <array>
#include <stdexcept>
#include <string> // needed for std::to_string() on macOS/BSD

#include "exceptions/null_pointer_exception.h"
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

    ObjectArray::ObjectArray(size_t length)
    {
        arr_ = new std::shared_ptr<const perlang::Object>[length];
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
        if (this == nullptr) {
            throw NullPointerException();
        }

        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        return arr_[index];
    }

    void ObjectArray::set(size_t index, const std::shared_ptr<Object>& value)
    {
        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        arr_[index] = value;
    }

    size_t ObjectArray::length() const
    {
        return length_;
    }
}
