#include <cstring>
#include <stdexcept>

#include "utf8_string.h"

namespace perlang
{
    UTF8String UTF8String::from_static_string(const char* s)
    {
        if (s == nullptr) {
            throw std::invalid_argument("string argument cannot be null");
        }

        auto result = UTF8String();
        result.bytes_ = s;
        result.length_ = strlen(s);

        return result;
    }

    UTF8String::UTF8String()
    {
        // Set these to ensure we have predictable defaults. Note that this constructor must ONLY be used from factory
        // methods, which set these fields to their appropriate values.
        bytes_ = nullptr;
        length_ = -1;
    }

    const char* UTF8String::bytes() const
    {
        return bytes_;
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
}