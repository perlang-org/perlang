#include <cstring>
#include <stdexcept>
#include <memory>

#include "bigint.h"
#include "utf8_string.h"

namespace perlang
{
    std::shared_ptr<const UTF8String> UTF8String::from_static_string(const char* s)
    {
        if (s == nullptr) {
            throw std::invalid_argument("string argument cannot be null");
        }

        auto result = new UTF8String(s, strlen(s), false);

        return std::shared_ptr<UTF8String>(result);
    }

    std::shared_ptr<const UTF8String> UTF8String::from_owned_string(const char* s, size_t length)
    {
        if (s == nullptr) {
            throw std::invalid_argument("string argument cannot be null");
        }

        auto result = new UTF8String(s, length, true);

        return std::shared_ptr<UTF8String>(result);
    }

    std::shared_ptr<const UTF8String> UTF8String::from_copied_string(const char* str)
    {
        if (str == nullptr) {
            throw std::invalid_argument("str argument cannot be null");
        }

        // Create a new buffer and copy the string into it. Since we need to know the length anyway, we can use
        // memcpy() instead of strcpy() to avoid an extra iteration over the string.
        size_t length = strlen(str);
        char* new_str = (char*)malloc(length + 1);
        memcpy(new_str, str, length);
        new_str[length] = '\0';

        auto result = new UTF8String(new_str, length, true);

        return std::shared_ptr<UTF8String>(result);
    }

    UTF8String::UTF8String(const char* string, size_t length, bool owned)
    {
        bytes_ = string;
        length_ = length;
        owned_ = owned;
    }

    UTF8String::~UTF8String()
    {
        if (owned_) {
            free((void*)bytes_);
        }
    }

    const char* UTF8String::bytes() const
    {
        return bytes_;
    }

    size_t UTF8String::length() const
    {
        return length_;
    }

    bool UTF8String::operator==(const UTF8String& rhs) const
    {
        return bytes_ == rhs.bytes_ &&
               length_ == rhs.length_;
    }

    bool UTF8String::operator!=(const UTF8String& rhs) const
    {
        return !(rhs == *this);
    }

    std::shared_ptr<const String> UTF8String::operator+(const String& rhs) const
    {
        size_t length = this->length_ + rhs.length();
        char *bytes = (char*)malloc(length + 1);

        // TODO: This won't work once we bring in UTF16String into the picture.
        memcpy(bytes, this->bytes_, this->length_);
        memcpy((bytes + this->length_), rhs.bytes(), rhs.length());
        bytes[length] = '\0';

        return from_owned_string(bytes, length);
    }

    std::shared_ptr<const UTF8String> UTF8String::operator+(const UTF8String& rhs) const
    {
        size_t length = this->length_ + rhs.length();
        char *bytes = (char*)malloc(length + 1);

        memcpy(bytes, this->bytes_, this->length_);
        memcpy((bytes + this->length_), rhs.bytes(), rhs.length());
        bytes[length] = '\0';

        return from_owned_string(bytes, length);
    }

    std::shared_ptr<const String> UTF8String::operator+(const int64_t rhs) const
    {
        std::string str = std::to_string(rhs);

        size_t length = str.length() + this->length_;
        char *bytes = (char*)malloc(length + 1);

        memcpy(bytes, this->bytes_, this->length_);
        memcpy((bytes + this->length_), str.c_str(), str.length());
        bytes[length] = '\0';

        return from_owned_string(bytes, length);
    }

    std::shared_ptr<const String> UTF8String::operator+(const uint64_t rhs) const
    {
        std::string str = std::to_string(rhs);

        size_t length = str.length() + this->length_;
        char *bytes = (char*)malloc(length + 1);

        memcpy(bytes, this->bytes_, this->length_);
        memcpy((bytes + this->length_), str.c_str(), str.length());
        bytes[length] = '\0';

        return from_owned_string(bytes, length);
    }

    std::shared_ptr<const String> UTF8String::operator+(const BigInt& rhs) const
    {
        std::string str = rhs.to_string();

        size_t length = str.length() + this->length_;
        char *bytes = (char*)malloc(length + 1);

        memcpy(bytes, this->bytes_, this->length_);
        memcpy((bytes + this->length_), str.c_str(), str.length());
        bytes[length] = '\0';

        return from_owned_string(bytes, length);
    }

    std::shared_ptr<const UTF8String> operator+(const int64_t lhs, const UTF8String& rhs)
    {
        std::string str = std::to_string(lhs);
        size_t length = str.length() + rhs.length();
        char *bytes = (char*)malloc(length + 1);

        memcpy(bytes, str.c_str(), str.length());
        memcpy((bytes + str.length()), rhs.bytes(), rhs.length());
        bytes[length] = '\0';

        return UTF8String::from_owned_string(bytes, length);
    }

    std::shared_ptr<const UTF8String> operator+(const uint64_t lhs, const UTF8String& rhs)
    {
        std::string str = std::to_string(lhs);
        size_t length = str.length() + rhs.length();
        char *bytes = (char*)malloc(length + 1);

        memcpy(bytes, str.c_str(), str.length());
        memcpy((bytes + str.length()), rhs.bytes(), rhs.length());
        bytes[length] = '\0';

        return UTF8String::from_owned_string(bytes, length);
    }
}
