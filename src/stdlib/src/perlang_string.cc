#include <cstring>

#include "perlang_stdlib.h"

namespace perlang
{
    // Would be nice to inline this, but it's hard because of forward declaration of UTF8String in perlang_string.h and
    // circular dependencies. We'll have to live with this until we can reimplement it in Perlang... :)
    bool String::operator==(UTF8String& rhs) const
    {
        return *this == static_cast<String*>(&rhs);
    }

    bool String::operator==(String* rhs) const
    {
        const UTF8String* utf8_lhs;
        const UTF8String* utf8_rhs;

        const ASCIIString* ascii_lhs;
        const ASCIIString* ascii_rhs;

        if ((utf8_lhs = dynamic_cast<const UTF8String*>(this)) != nullptr && (utf8_rhs = dynamic_cast<UTF8String*>(rhs)) != nullptr) {
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
            // the backing byte buffers are guranteed to work.
            return memcmp(utf8_lhs->bytes(), utf8_rhs->bytes(), length()) == 0;
        }
        else if ((ascii_lhs = dynamic_cast<const ASCIIString*>(this)) != nullptr && (ascii_rhs = dynamic_cast<ASCIIString*>(rhs)) != nullptr)
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
            // the backing byte buffers are guranteed to work.
            return memcmp(ascii_lhs->bytes(), ascii_rhs->bytes(), length()) == 0;
        }
        else {
            // The strings are of different types => consider them unequal. In the future, we want to support
            // "ASCII-only" strings stored in UTF8String instances (i.e. UTF8Strings with only 7-bit content), and
            // compare them semantically, regardless of whether the types exactly match or not.
            return false;
        }
    }

    bool String::operator!=(const String& rhs) const
    {
        return !(*this == rhs);
    }

    bool String::operator!=(String& rhs) const
    {
        return !(*this == rhs);
    }

    bool String::operator!=(UTF8String& rhs) const
    {
        return !(*this == rhs);
    }

    bool String::operator!=(String* rhs) const
    {
        return !(*this == rhs);
    }
}
