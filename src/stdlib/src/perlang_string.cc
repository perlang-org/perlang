#include <cstring>

#include "perlang_stdlib.h"
#include "perlang_string.h"

namespace perlang
{
    std::shared_ptr<const String> String::to_string() const
    {
        return shared_from_this();
    }

    // Would be nice to inline this, but it's hard because of forward declaration of UTF8String in perlang_string.h and
    // circular dependencies. We'll have to live with this until we can reimplement it in Perlang... :)
    bool String::operator==(UTF8String& rhs)
    {
        return *this == static_cast<String*>(&rhs);
    }

    bool String::operator==(String* rhs)
    {
        auto* utf8_lhs = dynamic_cast<UTF8String*>(this);
        auto* utf8_rhs = dynamic_cast<UTF8String*>(rhs);

        const auto* ascii_lhs = dynamic_cast<const ASCIIString*>(this);
        const auto* ascii_rhs = dynamic_cast<ASCIIString*>(rhs);

        if (utf8_lhs != nullptr && utf8_rhs != nullptr) {
            // If the length differs, there's no way the strings can match.
            if (utf8_lhs->length() != utf8_rhs->length()) {
                return false;
            }

            // Pointing at the same backing byte array means the strings are equal.
            if (utf8_lhs->bytes() == utf8_rhs->bytes()) {
                return true;
            }

            // memcmp() is an unsafe operation, but since we have verified that both strings have the expected length,
            // we should be on the safe side by now. The types have been validated to both be UTF8String, so comparing
            // the backing byte buffers are guaranteed to work.
            return memcmp(utf8_lhs->bytes(), utf8_rhs->bytes(), length()) == 0;
        }
        else if (ascii_lhs != nullptr && ascii_rhs != nullptr)
        {
            // If the length differs, there's no way the strings can match.
            if (ascii_lhs->length() != ascii_rhs->length()) {
                return false;
            }

            // Pointing at the same backing byte array means the strings are equal.
            if (ascii_lhs->bytes() == ascii_rhs->bytes()) {
                return true;
            }

            // memcmp() is an unsafe operation, but since we have verified that both strings have the expected length,
            // we should be on the safe side by now. The types have been validated to both be UTF8String, so comparing
            // the backing byte buffers are guaranteed to work.
            return memcmp(ascii_lhs->bytes(), ascii_rhs->bytes(), length()) == 0;
        }
        else if (ascii_lhs != nullptr && utf8_rhs != nullptr) {
            // If the UTF8 string contains one or more non-ASCII characters, it's logically impossible for the strings
            // to match.
            if (!utf8_rhs->is_ascii()) {
                return false;
            }

            // Also, if the length differs, there's no way the strings can match.
            if (ascii_lhs->length() != utf8_rhs->length()) {
                return false;
            }

            // Pointing at the same backing byte array means the strings are equal.
            if (ascii_lhs->bytes() == utf8_rhs->bytes()) {
                return true;
            }

            // memcmp() is an unsafe operation, but since we have verified that both strings have the expected length,
            // we should be on the safe side by now. Both of the types have been validated to contain ASCII-only
            // content, so comparing the backing byte buffers are guaranteed to work.
            return memcmp(ascii_lhs->bytes(), utf8_rhs->bytes(), length()) == 0;
        }
        else if (utf8_lhs != nullptr && ascii_rhs != nullptr) {
            // Identical to the previous branch, but it's the left-hand side that is a UTF8String instead of the
            // right-hand side.
            if (!utf8_lhs->is_ascii()) {
                return false;
            }

            if (utf8_lhs->length() != ascii_rhs->length()) {
                return false;
            }

            if (utf8_lhs->bytes() == ascii_rhs->bytes()) {
                return true;
            }

            return memcmp(utf8_lhs->bytes(), ascii_rhs->bytes(), length()) == 0;
        }
        else {
            // The strings are of different types => consider them unequal. In the future, we want to support
            // "ASCII-only" strings stored in UTF8String instances (i.e. UTF8Strings with only 7-bit content), and
            // compare them semantically, regardless of whether the types exactly match or not.
            return false;
        }
    }

    bool String::operator!=(const String& rhs)
    {
        return !(*this == rhs);
    }

    bool String::operator!=(String& rhs)
    {
        return !(*this == rhs);
    }

    bool String::operator!=(UTF8String& rhs)
    {
        return !(*this == rhs);
    }

    bool String::operator!=(String* rhs)
    {
        return !(*this == rhs);
    }
}
