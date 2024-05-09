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

        // TODO: This won't work once we bring in UTF16String into the picture.
        memcpy((void*)bytes, this->bytes_, this->length_);
        memcpy((void*)(bytes + this->length_), rhs.bytes(), rhs.length());
        bytes[length] = '\0';

        return from_owned_string(bytes, length);
    }

    std::shared_ptr<const ASCIIString> ASCIIString::operator+(const ASCIIString& rhs) const
    {
        // The alternative to copy-paste here would be to use a bunch of casting.

        size_t length = this->length_ + rhs.length();
        char *bytes = new char[length + 1];

        memcpy((void*)bytes, this->bytes_, this->length_);
        memcpy((void*)(bytes + this->length_), rhs.bytes(), rhs.length());
        bytes[length] = '\0';

        return from_owned_string(bytes, length);
    }

    std::shared_ptr<const String> ASCIIString::operator+(int64_t rhs) const
    {
        std::string str = std::to_string(rhs);

        size_t length = str.length() + this->length_;
        char *bytes = new char[length + 1];

        memcpy((void*)bytes, this->bytes_, this->length_);
        memcpy((void*)(bytes + this->length_), str.c_str(), str.length());
        bytes[length] = '\0';

        return from_owned_string(bytes, length);
    }

    std::shared_ptr<const String> ASCIIString::operator+(uint64_t rhs) const
    {
        std::string str = std::to_string(rhs);

        size_t length = str.length() + this->length_;
        char *bytes = new char[length + 1];

        memcpy((void*)bytes, this->bytes_, this->length_);
        memcpy((void*)(bytes + this->length_), str.c_str(), str.length());
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

    std::shared_ptr<const ASCIIString> operator+(const int64_t lhs, const ASCIIString& rhs)
    {
        std::string str = std::to_string(lhs);
        size_t length = str.length() + rhs.length();
        char *bytes = new char[length + 1];

        memcpy((void*)bytes, str.c_str(), str.length());
        memcpy((void*)(bytes + str.length()), rhs.bytes(), rhs.length());
        bytes[length] = '\0';

        return ASCIIString::from_owned_string(bytes, length);
    }

    std::shared_ptr<const ASCIIString> operator+(const uint64_t lhs, const ASCIIString& rhs)
    {
        std::string str = std::to_string(lhs);
        size_t length = str.length() + rhs.length();
        char *bytes = new char[length + 1];

        memcpy((void*)bytes, str.c_str(), str.length());
        memcpy((void*)(bytes + str.length()), rhs.bytes(), rhs.length());
        bytes[length] = '\0';

        return ASCIIString::from_owned_string(bytes, length);
    }
}
