#include <stdexcept>
#include <vector>

#include "bigint.h"
#include "utf8_string.h"
#include "utf16_string.h"
#include "exceptions/null_pointer_exception.h"
#include "internal/string_utils.h"

namespace perlang
{
    std::unique_ptr<perlang::UTF16String> UTF16String::from_owned_string(std::vector<uint16_t> s)
    {
        auto result = new UTF16String(s);

        return std::unique_ptr<UTF16String>(result);
    }

    std::unique_ptr<UTF16String> UTF16String::from_copied_string(const char* str)
    {
        if (str == nullptr) {
            throw std::invalid_argument("'str' argument cannot be null");
        }

        return from_copied_string(str, strlen(str));
    }

    std::unique_ptr<UTF16String> UTF16String::from_copied_string(const char* str, size_t length)
    {
        auto utf8_str = UTF8String::from_copied_string(str, length);
        return utf8_str->as_utf16();
    }

    UTF16String::UTF16String(std::vector<uint16_t> string)
    {
        data_ = std::move(string);
    }

    UTF16String::~UTF16String() = default;

    const char* UTF16String::bytes() const
    {
        return (const char*)data_.data();
    }

    std::unique_ptr<const char[]> UTF16String::release_bytes()
    {
        throw std::logic_error("UTF16String::release_bytes() is not supported; this class uses another data structure under the hood");
    }

    size_t UTF16String::length() const
    {
        // The vector always contains an extra NUL terminating character for now, because our print() implementation needs it.
        return data_.size() - 1;
    }

    bool UTF16String::is_ascii()
    {
        // Note that this is susceptible to data races; two threads could enter this method simultaneously. However,
        // this is considered tolerable. Either one of them will "win" and set the is_ascii_ value accordingly; the data
        // is immutable, so they will inevitably end up with the same result anyway.

        if (is_ascii_ != nullptr)
            return *is_ascii_;

        for (size_t i = 0; i < data_.size(); i++) {
            if (data_[i] > 127) {
                is_ascii_ = std::make_unique<bool>(false);
                return *is_ascii_;
            }
        }

        // No uint16 elements with bit 7-15 set => this is an ASCII string.
        is_ascii_ = std::make_unique<bool>(true);
        return *is_ascii_;
    }

    std::unique_ptr<UTF16String> UTF16String::as_utf16() const
    {
        // Making a copy here is incredibly inefficient, but there's really no way to return a _unique_ pointer here
        // without making a new copy. Future improvements in this area could be to elaborate on if std::shared_ptr()
        // could be an option; it might not be trivial.
        return copy();
    }

    char16_t UTF16String::operator[](size_t index) const
    {
        if (this == nullptr) {
            throw NullPointerException();
        }

        if (index < length()) {
            // Since we have performed the bounds checking manually now, we can now use the more "dangerous" index
            // operator rather than at(), which would perform bounds checking again.
            return data_[index];
        }
        else {
            throw std::out_of_range(fmt::format("Index {0} is out-of-bounds for a string with length {1} (valid range: 0..{2})", index, length(), length() - 1));
        }

    }

    std::unique_ptr<String> UTF16String::operator+(String&) const
    {
        throw std::logic_error("operator+(String&) is not yet implemented");

        // TODO: Unsure if this will work
        // No need to reinvent the wheel
//        return this + rhs.as_utf16().get();
//
//        size_t length = this->length_ + rhs.length();
//        char *bytes = new char[length + 1];
//
//        memcpy(bytes, this->bytes_.get(), this->length_);
//        memcpy((bytes + this->length_), rhs.bytes(), rhs.length());
//        bytes[length] = '\0';
//
//        return from_owned_string(bytes, length);
    }

    std::unique_ptr<String> UTF16String::operator+(const int64_t rhs) const
    {
        std::string str = std::to_string(rhs);
        return *this + str;
    }

    std::unique_ptr<String> UTF16String::operator+(const uint64_t rhs) const
    {
        std::string str = std::to_string(rhs);
        return *this + str;
    }

    std::unique_ptr<String> UTF16String::operator+(float rhs) const
    {
        std::string str = internal::float_to_string(rhs);
        return *this + str;
    }

    std::unique_ptr<String> UTF16String::operator+(double rhs) const
    {
        std::string str = internal::double_to_string(rhs);
        return *this + str;
    }

    std::unique_ptr<String> UTF16String::operator+(const BigInt& rhs) const
    {
        std::string str = rhs.to_string();
        return *this + str;
    }

    std::unique_ptr<String> UTF16String::operator+(const std::string& rhs) const
    {
        // Note: the new string is deliberately not NUL-terminated, since there is little point in NUL-terminating UTF16
        // strings.
        size_t length = this->data_.size() + rhs.length();
        auto bytes = std::vector<uint16_t>(length);

        memcpy(bytes.data(), this->data_.data(), this->data_.size() * 2);

        size_t rhs_length = rhs.length();
        auto rhs_string = rhs.c_str();

        for (size_t i = 0; i < rhs_length; i++) {
            if ((uint8_t)rhs_string[i] > 127) {
                throw std::logic_error("UTF16String::operator+(const std::string&) currently only supports concatenation with ASCII strings");
            }

            bytes[this->data_.size() + i] = (uint8_t)rhs_string[i];
        }

        return from_owned_string(bytes);
    }
}
