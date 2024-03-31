#include <cstring>
#include <stdexcept>
#include <string> // needed for std::to_string() on macOS

#include "ascii_string.h"

namespace perlang
{
    ASCIIString ASCIIString::from_static_string(const char* s)
    {
        if (s == nullptr) {
            throw std::invalid_argument("string argument cannot be null");
        }

        auto result = ASCIIString();
        result.bytes_ = s;
        result.length_ = strlen(s);

        return result;
    }

    ASCIIString::ASCIIString()
    {
        // Set these to ensure we have predictable defaults. Note that this constructor must ONLY be used from factory
        // methods, which set these fields to their appropriate values.
        bytes_ = nullptr;
        length_ = -1;
    }

    const char* ASCIIString::bytes() const
    {
        return bytes_;
    }
    bool ASCIIString::operator==(const ASCIIString& rhs) const
    {
        return bytes_ == rhs.bytes_ &&
               length_ == rhs.length_;
    }
    bool ASCIIString::operator!=(const ASCIIString& rhs) const
    {
        return !(rhs == *this);
    }

    char ASCIIString::operator[](size_t index) const
    {
        if (index < length_) {
            return bytes_[index];
        }
        else {
            throw std::out_of_range("Index " + std::to_string(index) + " is out-of-bounds for a string with length " + std::to_string(this->length_));
        }
    }
}
