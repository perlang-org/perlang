#pragma once

#include <cstddef>

#include "perlang_string.h"

namespace perlang
{
    class ASCIIString : public String
    {
     public:
        // Creates a new ASCIIString from a "static string", like a string constant. The static string is guaranteed to
        // have "static lifetime" (in Rust terms); in other words, that it exists during the remaining lifetime of the
        // running program. We also extend this to presume that the content of the string remains the same during this
        // whole lifetime.
        //
        // Because of the above assumptions, we know that we can make the new ASCIIString constructed from the `s`
        // parameter "borrow" the actual bytes_ used by the string. Since no deallocation will take place, and no
        // mutation, copying the string at this point would just waste CPU cycles for no added benefit.
        static ASCIIString from_static_string(const char* s);

        // Returns the backing byte array for this ASCIIString. This method is generally to be avoided; it is safer to
        // use the ASCIIString throughout the code and only call this when you really must.
        [[nodiscard]]
        const char* bytes() const override;

        // Compares the equality of two `ASCIIString`s, returning `true` if they point to the same backing byte array
        // and have the same length. Note that this method does *not* compare referential equality; two different
        // `ASCIIString` instances pointing at the same backing byte array are considered equal in Perlang. For all
        // practical matters, they can be considered interchangeable because strings are guaranteed to be immutable for
        // the whole lifetime of the program.
        bool operator==(const ASCIIString& rhs) const;

        // Compares the equality of two `ASCIIString`s, returning `true` if they are non-equal. See the `operator==`
        // documentation for more semantic details about the implementation.
        bool operator!=(const ASCIIString& rhs) const;

        // Indexes the string, returning the character at the given position. Note that this method performs bounds
        // checking; attempting to read outside the string will result in an exception.
        char operator[](size_t index) const;

     private:
        // Private constructor for creating a `null` string, not yet initialized with any sensible content.
        ASCIIString();

        // The backing byte array for this string. This is to be considered immutable and MUST NOT be modified at any
        // point. There might be multiple ASCIIString objects pointing to the same `bytes_`, so modifying one of them
        // would unintentionally spread the modifications to these other objects too.
        const char* bytes_;

        // The length of the string in bytes, excluding the terminating `NUL` character.
        size_t length_;
    };
}
