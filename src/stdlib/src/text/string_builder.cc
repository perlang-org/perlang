#include <stdexcept>
#include <cstring>

#include "ascii_string.h"
#include "utf8_string.h"
#include "text/string_builder.h"

namespace perlang::text
{
    StringBuilder::StringBuilder()
    {
        buffer_ = new char[DEFAULT_BUFFER_SIZE]{ 0 };
    }

    StringBuilder::~StringBuilder()
    {
        delete[] buffer_;
    }

    void StringBuilder::append(const String& str)
    {
        if (current_position_ + str.length() >= DEFAULT_BUFFER_SIZE) {
            throw std::runtime_error("StringBuilder buffer_ is full. Dynamically resizing it is not yet implemented.");
        }

        memcpy((void*)&buffer_[current_position_], str.bytes(), str.length());
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
        return UTF8String::from_copied_string(buffer_, length_);
    }
}
