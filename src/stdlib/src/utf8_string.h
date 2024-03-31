#pragma once

#include <cstddef>

#include "perlang_string.h"

namespace perlang
{
    class UTF8String : public String
    {
     public:
        // Creates a new UTF8String from a "static string", like a string constant. The static string is guaranteed to
        // have "static lifetime" (in Rust terms); in other words, that it exists during the remaining lifetime of the
        // running program. We also extend this to presume that the content of the string remains the same during this
        // whole lifetime.
        //
        // Because of the above assumptions, we know that we can make the new UTF8String constructed from the `s`
        // parameter "borrow" the actual bytes_ used by the string. Since no deallocation will take place, and no
        // mutation, copying the string at this point would just waste CPU cycles for no added benefit.
        static UTF8String from_static_string(const char* s);

        // Returns the backing byte array for this UTF8String. This method is generally to be avoided; it is safer to
        // use the UTF8String throughout the code and only call this when you really must.
        [[nodiscard]]
        const char* bytes() const override;

        // Compares the equality of two `UTF8String`s, returning `true` if they point to the same backing byte array
        // and have the same length. Note that this method does *not* compare referential equality; two different
        // `UTF8String` instances pointing at the same backing byte array are considered equal in Perlang. For all
        // practical matters, they can be considered interchangeable because strings are guaranteed to be immutable for
        // the whole lifetime of the program.
        bool operator==(const UTF8String& rhs) const;

        // Compares the equality of two `UTF8String`s, returning `true` if they are non-equal. See the `operator==`
        // documentation for more semantic details about the implementation.
        bool operator!=(const UTF8String& rhs) const;

     private:
        // Private constructor for creating a `null` string, not yet initialized with any sensible content.
        UTF8String();

        // The backing byte array for this string. This is to be considered immutable and MUST NOT be modified at any
        // point. There might be multiple UTF8String objects pointing to the same `bytes_`, so modifying one of them
        // would unintentionally spread the modifications to these other objects too.
        const char* bytes_;

        // The length of the string in bytes, excluding the terminating `NUL` character.
        size_t length_;
    };
}
