#include <cstring>
#include <memory>
#include <stdexcept>

#include "bigint.h"
#include "internal/string_utils.h"
#include "utf8_string.h"

#define TWO_BYTE_UTF8(c)   (((c) & 0b11100000) == 0b11000000)
#define THREE_BYTE_UTF8(c) (((c) & 0b11110000) == 0b11100000)
#define FOUR_BYTE_UTF8(c)  (((c) & 0b11111000) == 0b11110000)

#define TWO_BYTE_UTF8_WITHOUT_MASK(c)   ((c) & 0b00011111)
#define THREE_BYTE_UTF8_WITHOUT_MASK(c) ((c) & 0b00001111)
#define FOUR_BYTE_UTF8_WITHOUT_MASK(c)  ((c) & 0b00000111)

// This is present in all bytes of a UTF-8 sequence except for the first one
#define IS_UTF8_SEQUENCE_MASK(c) (((c) & 0b11000000) == 0b10000000)
#define UTF8_WITHOUT_SEQUENCE_MASK(c) ((c) & 0b00111111)

namespace perlang
{
    std::unique_ptr<UTF8String> UTF8String::from_static_string(const char* s)
    {
        if (s == nullptr) {
            throw std::invalid_argument("'s' argument cannot be null");
        }

        auto result = new UTF8String(s, strlen(s), false);

        return std::unique_ptr<UTF8String>(result);
    }

    std::unique_ptr<UTF8String> UTF8String::from_owned_string(const char* s, size_t length)
    {
        if (s == nullptr) {
            throw std::invalid_argument("'s' argument cannot be null");
        }

        auto result = new UTF8String(s, length, true);

        return std::unique_ptr<UTF8String>(result);
    }

    std::unique_ptr<UTF8String> UTF8String::from_copied_string(const char* str)
    {
        if (str == nullptr) {
            throw std::invalid_argument("'str' argument cannot be null");
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

    UTF8String::UTF8String()
    {
        bytes_ = std::unique_ptr<const char[]>(nullptr);
        length_ = 0;
        owned_ = false;
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

    bool UTF8String::is_ascii()
    {
        // Note that this is susceptible to data races; two threads could enter this method simultaneously. However,
        // this is considered tolerable. Either one of them will "win" and set the is_ascii_ value accordingly; the data
        // is immutable, so they will inevitably end up with the same result anyway.

        if (is_ascii_ != nullptr)
            return *is_ascii_;

        for (size_t i = 0; i < length_; i++) {
            if ((uint8_t)bytes_[i] > 127) {
                is_ascii_ = std::make_unique<bool>(false);
                return *is_ascii_;
            }
        }

        // No bytes with bit 7 (value 128) set => this is an ASCII string.
        is_ascii_ = std::make_unique<bool>(true);
        return *is_ascii_;
    }

    std::unique_ptr<UTF16String> UTF8String::as_utf16() const
    {
        // This would technically not be *required* to be handled separately; I was at point thinking of using a
        // "do/while" construct below which would have been prone to buffer overruns, which is why I added it. Either
        // way, special-casing it so we could at some point return a (pre-instantiated) UTF16String::empty() or
        // something might not hurt.
        if (length_ == 0) {
            return UTF16String::from_copied_string("");
        }

        // The max size of the buffer needed is twice the size of the UTF-8 string, if we presume that 100% of its
        // content is ASCII (One byte per character in ASCII => 16 bits in UTF-16). For UTF-8 encoded characters, a
        // maximum of 4 bytes can be used per RFC3629. Each UTF-8 can be represented in at most two UTF-16 code points
        // (=4 bytes), so such characters will fit in the above.
        //
        // This is likely to be too big for most strings we encounter, so we allocate a new array at the end of the
        // right size. The downside of this is the extra allocation; the upside is that we don't need an extra iteration
        // over the string just to determine its length before the allocation. Future improvements here could be to
        // stack-allocate the intermediate buffer if the string is below a given threshold (say, 1KB or 8KB) to minimize
        // the number of allocations further.
        auto data = std::vector<uint16_t>(length_ * 2);

        size_t i = 0;
        size_t new_length = 0;

        while (i < length_) {
            uint8_t c = bytes_[i];

            if (c <= 127) {
                // This is an ASCII character; no special measures are needed
                data[new_length] = c;
                i++;
                new_length++;
            }
            else {
                // There are three valid scenarios at this point, as described nicely here:
                // https://dev.to/emnudge/decoding-utf-8-3947 (examples are in binary):
                //
                // - Starting with 110 - UTF-8 code point with 2 bytes
                // - Starting with 1110 - code point with 3 bytes
                // - Starting with 11110 - code point with 4 bytes
                //
                // Anything else (like 10) is to be considered invalid; it's only expected in the subsequent bytes.
                // Because we can make those assumptions, this is now implemented as a simple list of if/else
                // statements. We also need to do bounds checking, to ensure we don't cause a buffer overrun.
                if (TWO_BYTE_UTF8(c)) {
                    if (i + 1 >= length_) {
                        throw std::invalid_argument("Truncated UTF-8 sequence encountered (string was too short to fit two bytes of expected UTF-8 data)");
                    }

                    uint8_t d = bytes_[i + 1];

                    if (!IS_UTF8_SEQUENCE_MASK(d)) {
                        throw std::invalid_argument("Invalid UTF-8 sequence encountered (second byte lacks UTF-8 mask)");
                    }

                    uint16_t target_char = (TWO_BYTE_UTF8_WITHOUT_MASK(c) << 6) +
                                           UTF8_WITHOUT_SEQUENCE_MASK(d);

                    // "Overlong" sequences must be treated as invalid per RFC 3629
                    if (target_char < 128) {
                        throw std::invalid_argument("Invalid UTF-8 sequence encountered (encoded using more bytes than necessary)");
                    }

                    data[new_length] = target_char;

                    i += 2;
                    new_length++;
                }
                else if (THREE_BYTE_UTF8(c)) {
                    if (i + 2 >= length_) {
                        throw std::invalid_argument("Truncated UTF-8 sequence encountered (string was too short to fit three bytes of expected UTF-8 data)");
                    }

                    uint8_t d = bytes_[i + 1];
                    uint8_t e = bytes_[i + 2];

                    if (!IS_UTF8_SEQUENCE_MASK(d) || !IS_UTF8_SEQUENCE_MASK(e)) {
                        throw std::invalid_argument("Invalid UTF-8 sequence encountered (one or more byte(s) lacks UTF-8 mask)");
                    }

                    uint16_t target_char = (THREE_BYTE_UTF8_WITHOUT_MASK(c) << 12) +
                                           (UTF8_WITHOUT_SEQUENCE_MASK(d) << 6) +
                                           UTF8_WITHOUT_SEQUENCE_MASK(e);

                    // "Overlong" sequences must be treated as invalid per RFC 3629
                    if (target_char < 2048) {
                        throw std::invalid_argument("Invalid UTF-8 sequence encountered (encoded using more bytes than necessary)");
                    }

                    data[new_length] = target_char;

                    i += 3;
                    new_length++;
                }
                else if (FOUR_BYTE_UTF8(c)) {
                    if (i + 3 >= length_) {
                        throw std::invalid_argument("Truncated UTF-8 sequence encountered (string was too short to fit four bytes of expected UTF-8 data)");
                    }

                    uint8_t d = bytes_[i + 1];
                    uint8_t e = bytes_[i + 2];
                    uint8_t f = bytes_[i + 3];

                    if (!IS_UTF8_SEQUENCE_MASK(d) || !IS_UTF8_SEQUENCE_MASK(e) || !IS_UTF8_SEQUENCE_MASK(f)) {
                        throw std::invalid_argument("Invalid UTF-8 sequence encountered (one or more  byte(s) lacks UTF-8 mask)");
                    }

                    // Note that the target character is a surrogate pair in this case, so we need to store two
                    // characters in the resulting array.
                    uint32_t target_char = (FOUR_BYTE_UTF8_WITHOUT_MASK(c) << 18) +
                                           (UTF8_WITHOUT_SEQUENCE_MASK(d) << 12) +
                                           (UTF8_WITHOUT_SEQUENCE_MASK(e) << 6) +
                                           UTF8_WITHOUT_SEQUENCE_MASK(f);

                    // "Overlong" sequences must be treated as invalid per RFC 3629
                    if (target_char < 65536) {
                        throw std::invalid_argument("Invalid UTF-8 sequence encountered (encoded using more bytes than necessary)");
                    }

                    if (target_char > 0x10FFFF) {
                        throw std::invalid_argument("Invalid UTF-8 sequence encountered (code point exceeds maximum allowed value of 0x10FFFF)");
                    }

                    // Convert the code point to a surrogate pair
                    target_char -= 0x10000;
                    data[new_length] = (target_char >> 10) + 0xD800;        // High surrogate
                    data[new_length + 1] = (target_char & 0x3FF) + 0xDC00;  // Low surrogate

                    i += 4;
                    new_length += 2;
                }
                else {
                    throw std::invalid_argument("Invalid UTF-8 sequence encountered (first byte does not match any known UTF-8 encoding scheme)");
                }
            }
        }

        // Shrink the std::vector to the actual size we need, now that we know the actual length of the converted
        // string. The +1 part is because our print() implementation expects the string to be NUL-terminated for now.
        // We'll try to get rid of this limitation eventually.
        data.resize(new_length + 1);

        return UTF16String::from_owned_string(std::move(data));
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

    std::unique_ptr<String> UTF8String::operator+(String& rhs) const
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

    std::unique_ptr<String> UTF8String::operator+(const BigInt& rhs) const
    {
        std::string str = rhs.to_string();
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
