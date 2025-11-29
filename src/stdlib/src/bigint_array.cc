#include <memory.h>
#include <stdexcept>
#include <string>

#include "exceptions/null_pointer_exception.h"
#include "bigint_array.h"

namespace perlang
{
    BigIntArray::BigIntArray(std::initializer_list<BigInt> arr)
    {
        // Create a new buffer and copy the array into it.
        size_t length = arr.size();
        auto new_arr = new BigInt[length];

        for (size_t i = 0; i < length; i++) {
            new_arr[i] = data(arr)[i];
        }

        arr_ = new_arr;
        length_ = length;
        owned_ = true;
    }

    BigIntArray::BigIntArray(size_t length)
    {
        arr_ = new BigInt[length];
        length_ = length;
        owned_ = true;
    }

    BigIntArray::BigIntArray(BigInt* arr, size_t length, bool owned)
    {
        arr_ = arr;
        length_ = length;
        owned_ = owned;
    }

    BigIntArray::~BigIntArray()
    {
        if (owned_) {
            delete[] arr_;
        }
    }

    BigInt BigIntArray::operator[](size_t index) const
    {
        if (this == nullptr) {
            throw NullPointerException();
        }

        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        return arr_[index];
    }

    void BigIntArray::set(size_t index, const BigInt& value)
    {
        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        arr_[index] = value;
    }

    bool BigIntArray::contains(const BigInt& value) const
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
