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

        // TODO: Mark this string as "static" in some way, to ensure the destructor doesn't try to delete `bytes_`.
        auto result = UTF8String();
        result.bytes_ = s;
        result.length_ = strlen(s);

        return std::make_shared<UTF8String>(result);
    }

    UTF8String::UTF8String()
    {
        // Set these to ensure we have predictable defaults. Note that this constructor must ONLY be used from factory
        // methods, which set these fields to their appropriate values.
        bytes_ = nullptr;
        length_ = -1;
    }

    // TODO: Implement deallocation here for non-static strings, but MAKE SURE to keep a distinction between static and
    // TODO: non-static strings!
    UTF8String::~UTF8String() = default;

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
