#include <cstring>
#include <stdexcept>
#include <string> // needed for std::to_string() on macOS
#include <memory>

#include "ascii_string.h"

namespace perlang
{
    std::shared_ptr<const ASCIIString> ASCIIString::from_static_string(const char* s)
    {
        if (s == nullptr) {
            throw std::invalid_argument("string argument cannot be null");
        }

        auto result = new ASCIIString(s, strlen(s), false);

        return std::shared_ptr<ASCIIString>(result);
    }

    std::shared_ptr<const ASCIIString> ASCIIString::from_owned_string(const char* s, size_t length)
    {
        if (s == nullptr) {
            throw std::invalid_argument("string argument cannot be null");
        }

        auto result = new ASCIIString(s, length, true);

        return std::shared_ptr<ASCIIString>(result);
    }

    ASCIIString::ASCIIString(const char* string, size_t length, bool owned)
    {
        bytes_ = string;
        length_ = length;
        owned_ = owned;
    }

    ASCIIString::~ASCIIString()
    {
        if (owned_) {
            delete[] bytes_;
        }
    }

    const char* ASCIIString::bytes() const
    {
        return bytes_;
    }

    size_t ASCIIString::length() const
    {
        return length_;
    }

    bool ASCIIString::operator==(const ASCIIString& rhs) const
    {
        // TODO: Should return true for strings with different bytes_ values, as long as the bytes are equal!
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

    std::shared_ptr<const String> ASCIIString::operator+(const String& rhs) const
    {
        size_t length = this->length_ + rhs.length();
        char *bytes = new char[length + 1];

        memcpy((void*)bytes, this->bytes_, this->length_);
        memcpy((void*)(bytes + this->length_), rhs.bytes(), rhs.length());
        bytes[length] = '\0';

        return from_owned_string(bytes, length);
    }

    char ASCIIString::char_at(int index) const
    {
        // The reason this method exists is that the construct below would be slightly more awkward to generate from
        // PerlangCompiler.cs. It's easier to just add an extra char_at() method call when it visits the Expr.Index
        // expression.
        return (*this)[index];
    }
}
