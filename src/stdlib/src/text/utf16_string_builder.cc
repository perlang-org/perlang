#include "perlang_stdlib.h"

namespace perlang::text
{
    void UTF16StringBuilder::append(const String& str)
    {
        const auto utf16_string = str.as_utf16();

        for (size_t i = 0; i < utf16_string->length(); i++) {
            buffer_.push_back((uint16_t)(*utf16_string)[i]);
        }
    }

    void UTF16StringBuilder::append(char16_t c)
    {
        buffer_.push_back((uint16_t)c);
    }

    void UTF16StringBuilder::append_line(const String& str)
    {
        append(str);
        append('\n');
    }

    uint64_t UTF16StringBuilder::length() const
    {
        return buffer_.size();
    }

    std::unique_ptr<String> UTF16StringBuilder::to_string() const
    {
        return UTF16String::from_owned_string(buffer_);
    }
}
