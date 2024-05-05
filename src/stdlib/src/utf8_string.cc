#include <cstring>
#include <stdexcept>
#include <memory>

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

    UTF8String::UTF8String(const char* string, size_t length, bool owned)
    {
        bytes_ = string;
        length_ = length;
        owned_ = owned;
    }

    UTF8String::~UTF8String()
    {
        if (owned_) {
            delete[] bytes_;
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
        char *bytes = new char[length + 1];

        memcpy((void*)bytes, this->bytes_, this->length_);
        memcpy((void*)(bytes + this->length_), rhs.bytes(), rhs.length());
        bytes[length] = '\0';

        return from_owned_string(bytes, length);
    }
}
