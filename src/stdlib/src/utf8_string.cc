#include <cstring>
#include <memory>
#include <stdexcept>

#include "bigint.h"
#include "internal/string_utils.h"
#include "utf8_string.h"

namespace perlang
{
    std::unique_ptr<UTF8String> UTF8String::from_static_string(const char* s)
    {
        if (s == nullptr) {
            throw std::invalid_argument("string argument cannot be null");
        }

        auto result = new UTF8String(s, strlen(s), false);

        return std::unique_ptr<UTF8String>(result);
    }

    std::unique_ptr<UTF8String> UTF8String::from_owned_string(const char* s, size_t length)
    {
        if (s == nullptr) {
            throw std::invalid_argument("string argument cannot be null");
        }

        auto result = new UTF8String(s, length, true);

        return std::unique_ptr<UTF8String>(result);
    }

    std::unique_ptr<UTF8String> UTF8String::from_copied_string(const char* str)
    {
        if (str == nullptr) {
            throw std::invalid_argument("str argument cannot be null");
        }

        return from_copied_string(str, strlen(str));
    }

    std::unique_ptr<UTF8String> UTF8String::from_copied_string(const char* str, size_t length)
    {
        // Create a new buffer and copy the string into it. Since we know the length anyway, we can use memcpy() instead
        // of strcpy() to avoid an extra iteration over the string.
        char* new_str = new char[length + 1];
        memcpy(new_str, str, length);
        new_str[length] = '\0';

        auto result = new UTF8String(new_str, length, true);

        return std::unique_ptr<UTF8String>(result);
    }

    UTF8String::UTF8String(const char* string, size_t length, bool owned)
    {
        bytes_ = std::unique_ptr<const char[]>(string);
        length_ = length;
        owned_ = owned;
    }

    UTF8String::~UTF8String()
    {
        // HACK: This is an incredible hack... Because unique_ptr<> doesn't give us a way to override the deleter
        // function, we manually release control of the pointed-to memory here if we don't own it... :)
        if (!owned_) {
            bytes_.release();
        }
    }

    const char* UTF8String::bytes() const
    {
        return bytes_.get();
    }

    std::unique_ptr<const char[]> UTF8String::release_bytes()
    {
        return std::move(bytes_);
    }

    size_t UTF8String::length() const
    {
        return length_;
    }

    bool UTF8String::operator==(const UTF8String& rhs) const
    {
        if (bytes_ == rhs.bytes_ &&
            length_ == rhs.length_) {
            return true;
        }

        if (length_ != rhs.length_) {
            return false;
        }

        // We must make sure to use a NUL-safe method here, since UTF8 strings can regretfully contain NUL characters.
        return memcmp(bytes_.get(), rhs.bytes_.get(), length_) == 0;
    }

    bool UTF8String::operator!=(const UTF8String& rhs) const
    {
        return !(rhs == *this);
    }

    std::unique_ptr<String> UTF8String::operator+(const String& rhs) const
    {
        size_t length = this->length_ + rhs.length();
        char *bytes = new char[length + 1];

        // TODO: This won't work once we bring in UTF16String into the picture.
        memcpy(bytes, this->bytes_.get(), this->length_);
        memcpy((bytes + this->length_), rhs.bytes(), rhs.length());
        bytes[length] = '\0';

        return from_owned_string(bytes, length);
    }

    std::unique_ptr<UTF8String> UTF8String::operator+(const UTF8String& rhs) const
    {
        size_t length = this->length_ + rhs.length();
        char *bytes = new char[length + 1];

        memcpy(bytes, this->bytes_.get(), this->length_);
        memcpy((bytes + this->length_), rhs.bytes(), rhs.length());
        bytes[length] = '\0';

        return from_owned_string(bytes, length);
    }

    std::unique_ptr<String> UTF8String::operator+(const int64_t rhs) const
    {
        std::string str = std::to_string(rhs);
        return *this + str;
    }

    std::unique_ptr<String> UTF8String::operator+(const uint64_t rhs) const
    {
        std::string str = std::to_string(rhs);
        return *this + str;
    }

    std::unique_ptr<String> UTF8String::operator+(float rhs) const
    {
        std::string str = internal::float_to_string(rhs);
        return *this + str;
    }

    std::unique_ptr<String> UTF8String::operator+(double rhs) const
    {
        std::string str = internal::double_to_string(rhs);
        return *this + str;
    }

    std::unique_ptr<String> UTF8String::operator+(const std::string& rhs) const
    {
        size_t length = this->length_ + rhs.length();
        char *bytes = new char[length + 1];

        memcpy(bytes, this->bytes_.get(), this->length_);
        memcpy((bytes + this->length_), rhs.c_str(), rhs.length());
        bytes[length] = '\0';

        return from_owned_string(bytes, length);
    }

    std::unique_ptr<String> UTF8String::operator+(const BigInt& rhs) const
    {
        std::string str = rhs.to_string();
        return *this + str;
    }

    bool UTF8String::is_ascii()
    {
        // Note that this is susceptible to data races; two threads could enter this method simultaneously. However,
        // this is considered tolerable. Either one of them will "win" and set the is_ascii_ value accordingly; the data
        // is immutable, so they will inevitably end up with the same result anyway.

        if (is_ascii_ != nullptr)
            return *is_ascii_.get();

        for (size_t i = 0; i < length_; i++) {
            if ((uint8_t)bytes_[i] > 127) {
                is_ascii_ = std::make_unique<bool>(false);
                return *is_ascii_.get();
            }
        }

        // No bytes with bit 7 (value 128) encountered => this is an ASCII string.
        is_ascii_ = std::make_unique<bool>(true);
        return *is_ascii_.get();
    }

    std::unique_ptr<UTF8String> operator+(const int64_t lhs, const UTF8String& rhs)
    {
        std::string str = std::to_string(lhs);
        return str + rhs;
    }

    std::unique_ptr<UTF8String> operator+(const uint64_t lhs, const UTF8String& rhs)
    {
        std::string str = std::to_string(lhs);
        return str + rhs;
    }

    std::unique_ptr<UTF8String> operator+(const float lhs, const UTF8String& rhs)
    {
        std::string str = internal::float_to_string(lhs);
        return str + rhs;
    }

    std::unique_ptr<UTF8String> operator+(const double lhs, const UTF8String& rhs)
    {
        std::string str = internal::double_to_string(lhs);
        return str + rhs;
    }

    std::unique_ptr<UTF8String> operator+(const std::string& lhs, const UTF8String& rhs)
    {
        size_t length = lhs.length() + rhs.length();
        char *bytes = new char[length + 1];

        memcpy(bytes, lhs.c_str(), lhs.length());
        memcpy((bytes + lhs.length()), rhs.bytes(), rhs.length());
        bytes[length] = '\0';

        return UTF8String::from_owned_string(bytes, length);
    }
}
