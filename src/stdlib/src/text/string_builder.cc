#include <stdexcept>
#include <cstring>

#include "perlang_stdlib.h"

namespace perlang::text
{
    StringBuilder::StringBuilder()
    {
        buffer_capacity_ = DEFAULT_BUFFER_SIZE;
        buffer_ = new std::vector<char>(buffer_capacity_);
    }

    StringBuilder::~StringBuilder()
    {
        delete buffer_;
    }

    void StringBuilder::append(const String& str)
    {
        if (current_position_ + str.length() >= buffer_capacity_) {
            // Give the buffer enough size to fit the whole of str, and round it up to the closest 1KB boundary. For
            // some applications (adding large strings  to the StringBuilder), this will lead to a large number of
            // reallocations, but let's attempt to optimize for conserving memory consumption for now.
            buffer_capacity_ = ((buffer_capacity_ + str.length()) | 1023) + 1;
            buffer_->resize(buffer_capacity_);
        }

        memcpy((void*)&buffer_->data()[current_position_], str.bytes(), str.length());
        current_position_ += str.length();
        length_ += str.length();
    }

    void StringBuilder::append_line(const String& str)
    {
        append(str);
        append(*ASCIIString::from_static_string("\n"));
    }

    uint StringBuilder::length() const
    {
        return length_;
    }

    std::unique_ptr<perlang::String> StringBuilder::to_string()
    {
        // TODO: Make this return ASCIIString for cases when the string contains ASCII-only content.
        return UTF8String::from_copied_string(buffer_->data(), length_);
    }
}
