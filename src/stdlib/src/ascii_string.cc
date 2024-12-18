#include <cstring>
#include <stdexcept>
#include <string> // needed for std::to_string() on macOS
#include <memory>

#include "ascii_string.h"
#include "bigint.h"
#include "internal/string_utils.h"

namespace perlang
{
    std::unique_ptr<ASCIIString> ASCIIString::from_static_string(const char* str)
    {
        if (str == nullptr) {
            throw std::invalid_argument("string argument cannot be null");
        }

        // Cannot use std::make_shared() since it forces the ASCIIString constructor to be made public.
        auto result = new ASCIIString(str, strlen(str), false);

        return std::unique_ptr<ASCIIString>(result);
    }

    std::unique_ptr<ASCIIString> ASCIIString::from_owned_string(const char* str, size_t length)
    {
        if (str == nullptr) {
            throw std::invalid_argument("str argument cannot be null");
        }

        auto result = new ASCIIString(str, length, true);

        return std::unique_ptr<ASCIIString>(result);
    }

    std::unique_ptr<ASCIIString> ASCIIString::from_copied_string(const char* str)
    {
        if (str == nullptr) {
            throw std::invalid_argument("str argument cannot be null");
        }

        // Create a new buffer and copy the string into it. Since we need to know the length anyway, we can use
        // memcpy() instead of strcpy() to avoid an extra iteration over the string.
        size_t length = strlen(str);
        char* new_str = new char[length + 1];
        memcpy(new_str, str, length);
        new_str[length] = '\0';

        auto result = new ASCIIString(new_str, length, true);

        return std::unique_ptr<ASCIIString>(result);
    }

    ASCIIString::ASCIIString(const char* string, size_t length, bool owned)
    {
        for (size_t i = 0; i < length; i++) {
            // The uint8_t cast here is not pretty, but doing the "right" thing and accepting a const uint8_t* parameter
            // instead makes things more awkward (since C string literals are const char* by definition).
            if ((uint8_t)string[i] > 127) {
                // TODO: Try to include some content from the string in the exception here. It feels non-trivial since a
                // TODO: single 'char' is not a full UTF-8 character.
                throw std::invalid_argument("Non-ASCII character encountered at index " + std::to_string(i) + ". ASCIIStrings can only contain ASCII characters.");
            }
        }

        bytes_ = std::unique_ptr<const char[]>((const char*)string);
        length_ = length;
        owned_ = owned;
    }

    ASCIIString::~ASCIIString()
    {
        // HACK: This is an incredible hack... Because unique_ptr<> doesn't give us a way to override the deleter
        // function, we manually release control of the pointed-to memory here if we don't own it... :)
        if (!owned_) {
            bytes_.release();
        }
    }

    const char* ASCIIString::bytes() const
    {
        return bytes_.get();
    }

    std::unique_ptr<const char[]> ASCIIString::release_bytes()
    {
        return std::move(bytes_);
    }

    size_t ASCIIString::length() const
    {
        return length_;
    }

    bool ASCIIString::operator==(const ASCIIString& rhs) const
    {
        if (bytes_.get() == rhs.bytes_.get() &&
            length_ == rhs.length_) {
            return true;
        }

        if (length_ != rhs.length_) {
            return false;
        }

        // ASCII strings cannot contain NUL characters, so strcmp() should be safe for this case.
        return strcmp(bytes_.get(), rhs.bytes_.get()) == 0;
    }

    bool ASCIIString::operator!=(const ASCIIString& rhs) const
    {
        return !(rhs == *this);
    }

    char ASCIIString::operator[](size_t index) const
    {
        if (index < length_) {
            return bytes_.get()[index];
        }
        else {
            throw std::out_of_range("Index " + std::to_string(index) + " is out-of-bounds for a string with length " + std::to_string(this->length_));
        }
    }

    std::unique_ptr<String> ASCIIString::operator+(String& rhs) const
    {
        size_t length = this->length_ + rhs.length();
        char* bytes = new char[length + 1];

        // TODO: This won't work once we bring in UTF16String into the picture.
        memcpy(bytes, this->bytes_.get(), this->length_);
        memcpy((bytes + this->length_), rhs.bytes(), rhs.length());
        bytes[length] = '\0';

        auto* utf8_rhs = dynamic_cast<UTF8String*>(&rhs);

        // Depending on whether the right-hand string is ASCII-only or not, we need to construct different target types
        // here.
        if (rhs.is_ascii()) {
            return from_owned_string(bytes, length);
        }
        else if (utf8_rhs != nullptr) {
            return UTF8String::from_owned_string(bytes, length);
        }
        else {
            throw std::runtime_error("Unsupported string type encountered");
        }
    }

    std::unique_ptr<ASCIIString> ASCIIString::operator+(const ASCIIString& rhs) const
    {
        // Copy-paste is a bit ugly, but the alternative would perhaps also not be so pretty, calling the above method
        // and doing some semi-ugly casting of the result.

        size_t length = this->length_ + rhs.length();
        char *bytes = new char[length + 1];

        memcpy(bytes, this->bytes_.get(), this->length_);
        memcpy(bytes + this->length_, rhs.bytes(), rhs.length());
        bytes[length] = '\0';

        return from_owned_string(bytes, length);
    }

    std::unique_ptr<String> ASCIIString::operator+(int64_t rhs) const
    {
        std::string str = std::to_string(rhs);
        return *this + str;
    }

    std::unique_ptr<String> ASCIIString::operator+(uint64_t rhs) const
    {
        std::string str = std::to_string(rhs);
        return *this + str;
    }

    std::unique_ptr<String> ASCIIString::operator+(float rhs) const
    {
        std::string str = internal::float_to_string(rhs);
        return *this + str;
    }

    std::unique_ptr<String> ASCIIString::operator+(double rhs) const
    {
        std::string str = internal::double_to_string(rhs);
        return *this + str;
    }

    std::unique_ptr<String> ASCIIString::operator+(const BigInt& rhs) const
    {
        std::string str = rhs.to_string();
        return *this + str;
    }

    std::unique_ptr<String> ASCIIString::operator+(const std::string& rhs) const
    {
        size_t length = this->length_ + rhs.length();
        char *bytes = new char[length + 1];

        memcpy(bytes, this->bytes_.get(), this->length_);
        memcpy((bytes + this->length_), rhs.c_str(), rhs.length());
        bytes[length] = '\0';

        return from_owned_string(bytes, length);
    }

    std::unique_ptr<const ASCIIString> operator+(const int64_t lhs, const ASCIIString& rhs)
    {
        std::string str = std::to_string(lhs);
        return str + rhs;
    }

    std::unique_ptr<const ASCIIString> operator+(const uint64_t lhs, const ASCIIString& rhs)
    {
        std::string str = std::to_string(lhs);
        return str + rhs;
    }

    std::unique_ptr<const ASCIIString> operator+(const float lhs, const ASCIIString& rhs)
    {
        std::string str = internal::float_to_string(lhs);
        return str + rhs;
    }

    std::unique_ptr<const ASCIIString> operator+(const double lhs, const ASCIIString& rhs)
    {
        std::string str = internal::double_to_string(lhs);
        return str + rhs;
    }

    std::unique_ptr<const ASCIIString> operator+(const std::string& lhs, const ASCIIString& rhs)
    {
        size_t length = lhs.length() + rhs.length();
        char *bytes = new char[length + 1];

        memcpy(bytes, lhs.c_str(), lhs.length());
        memcpy((bytes + lhs.length()), rhs.bytes(), rhs.length());
        bytes[length] = '\0';

        return ASCIIString::from_owned_string(bytes, length);
    }
}
