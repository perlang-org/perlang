#pragma once

#include <cstddef>

#include "perlang_string.h"

// Forward declaration to avoid circular dependencies
class BigInt;

namespace perlang
{
    // A class for representing immutable ASCII strings.
    class ASCIIString : public String
    {
     public:
        // Creates a new ASCIIString from a "static string", like a string constant. The static string is guaranteed to
        // have "static lifetime" (in Rust terms); in other words, that it exists during the remaining lifetime of the
        // running program. We also extend this to presume that the content of the string remains the same during this
        // whole lifetime.
        //
        // Because of the above assumptions, we know that we can make the new ASCIIString constructed from the `str`
        // parameter "borrow" the actual bytes_ used by the string. Since no deallocation will take place, and no
        // mutation, copying the string at this point would just waste CPU cycles for no added benefit.
        [[nodiscard]]
        static std::unique_ptr<const ASCIIString> from_static_string(const char* str);

        // Creates a new ASCIIString from an "owned str", like a str that has been allocated on the heap. The
        // ownership of the memory is transferred to the ASCIIString, which is then responsible for deallocating the
        // memory when it is no longer needed (i.e. when no references to it remains).
        [[nodiscard]]
        static std::unique_ptr<const ASCIIString> from_owned_string(const char* str, size_t length);

        // Creates a new ASCIIString from an existing string, by copying its content to a new buffer allocated on the
        // heap. The ASCIIString class takes ownership of the newly allocated buffer, which will be deallocated when the
        // ASCIIString runs out of scope.
        [[nodiscard]]
        static std::unique_ptr<const ASCIIString> from_copied_string(const char* str);

     private:
        // Private constructor for creating a new ASCIIString from a C-style (NUL-terminated) string. The `owned`
        // parameter indicates whether the ASCIIString should take ownership of the memory it points to, and thus be
        // responsible for deallocating it when it is no longer needed.
        ASCIIString(const char* string, size_t length, bool owned);

     public:
        ~ASCIIString();

        // Returns the backing byte array for this ASCIIString. This method is generally to be avoided; it is safer to
        // use the ASCIIString throughout the code and only call this when you really must.
        [[nodiscard]]
        const char* bytes() const override;

        // The length of the string in bytes, excluding the terminating `NUL` character.
        [[nodiscard]]
        size_t length() const override;

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

        // Concatenates this string with another string. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        std::unique_ptr<const String> operator+(const String& rhs) const override;

        // Concatenates this string with another string. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        std::unique_ptr<const ASCIIString> operator+(const ASCIIString& rhs) const;

        // Concatenates this string with an int32. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        inline std::unique_ptr<const String> operator+(int32_t rhs) const override
        {
            return this->operator+(static_cast<int64_t>(rhs));
        }

        // Concatenates this string with an int64. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        std::unique_ptr<const String> operator+(int64_t rhs) const override;

        // Concatenates this string with a uint32. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        inline std::unique_ptr<const String> operator+(uint32_t rhs) const override
        {
            return this->operator+(static_cast<uint64_t>(rhs));
        }

        // Concatenates this string with a uint64. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        std::unique_ptr<const String> operator+(uint64_t rhs) const override;

        // Concatenates this string with a float. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        std::unique_ptr<const String> operator+(float rhs) const override;

        // Concatenates this string with a double. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        std::unique_ptr<const String> operator+(double rhs) const override;

        // Concatenates this string with a BigInt. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        std::unique_ptr<const String> operator+(const BigInt& rhs) const override;

     private:
        // The backing byte array for this string. This is to be considered immutable and MUST NOT be modified at any
        // point. There might be multiple ASCIIString objects pointing to the same `bytes_`, so modifying one of them
        // would unintentionally spread the modifications to these other objects too.
        const char* bytes_;

        // The length of the string in bytes, excluding the terminating `NUL` character.
        size_t length_;

        // A flag indicating whether this string owns the memory it points to. If this is true, the string is
        // responsible for deallocating the memory when it is no longer needed. If false, the string is borrowing the
        // memory from somewhere else, and should not deallocate it.
        bool owned_;
    };

    // Concatenate an int64+ASCIIString. The memory for the new string is allocated from the heap. This is a free
    // function, since the left-hand side is not an ASCIIString.
    [[nodiscard]]
    std::unique_ptr<const ASCIIString> operator+(int64_t lhs, const ASCIIString& rhs);

    // Concatenate a uint64+ASCIIString. The memory for the new string is allocated from the heap. This is a free
    // function, since the left-hand side is not an ASCIIString.
    [[nodiscard]]
    std::unique_ptr<const ASCIIString> operator+(uint64_t lhs, const ASCIIString& rhs);

    // Concatenate a float+ASCIIString. The memory for the new string is allocated from the heap. This is a free
    // function, since the left-hand side is not an ASCIIString.
    [[nodiscard]]
    std::unique_ptr<const ASCIIString> operator+(float lhs, const ASCIIString& rhs);

    // Concatenate a double+ASCIIString. The memory for the new string is allocated from the heap. This is a free
    // function, since the left-hand side is not an ASCIIString.
    [[nodiscard]]
    std::unique_ptr<const ASCIIString> operator+(double lhs, const ASCIIString& rhs);

    // Note: must come after the operator+ declarations above, since they are used in the following declarations. In
    // C++, the order of function declaration matters.

    // Concatenate an int32+ASCIIString. The memory for the new string is allocated from the heap. This is a free
    // function, since the left-hand side is not an ASCIIString.
    [[nodiscard]]
    inline std::unique_ptr<const ASCIIString> operator+(const int32_t lhs, const ASCIIString& rhs)
    {
        return static_cast<int64_t>(lhs) + rhs;
    }

    // Concatenate a uint32+ASCIIString. The memory for the new string is allocated from the heap. This is a free
    // function, since the left-hand side is not an ASCIIString.
    [[nodiscard]]
    inline std::unique_ptr<const ASCIIString> operator+(uint32_t lhs, const ASCIIString& rhs)
    {
        return static_cast<uint64_t>(lhs) + rhs;
    }
}
