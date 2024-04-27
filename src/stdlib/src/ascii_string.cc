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

        // TODO: Mark this string as "static" in some way, to ensure the destructor doesn't try to delete `bytes_`.
        auto result = ASCIIString();
        result.bytes_ = s;
        result.length_ = strlen(s);

        return std::make_shared<ASCIIString>(result);
    }

    ASCIIString::ASCIIString()
    {
        // Set these to ensure we have predictable defaults. Note that this constructor must ONLY be used from factory
        // methods, which set these fields to their appropriate values.
        bytes_ = nullptr;
        length_ = -1;
    }

    const char* ASCIIString::bytes() const
    {
        return bytes_;
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

    char ASCIIString::char_at(int index) const
    {
        // The reason this method exists is that the construct below would be slightly more awkward to generate from
        // PerlangCompiler.cs. It's easier to just add an extra char_at() method call when it visits the Expr.Index
        // expression.
        return (*this)[index];
    }
}
