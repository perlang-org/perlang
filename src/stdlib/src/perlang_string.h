#pragma once

#include <memory> // std::unique_ptr

// Forward declarations to avoid circular dependencies
class BigInt;

namespace perlang
{
    class UTF8String;

    // Abstract base class for all String types in Perlang
    class String
    {
     public:
        virtual ~String() = default;

        // TODO: Make this "internal" somehow. It should never be called from (Perlang) user code.

        // Returns the backing byte array for this String. This method is generally to be avoided; it is safer to use
        // the String throughout the code and only call this when you really must. If you call it, you
        // **MUST MUST MUST** not modify the data in any way, or use it after the lifetime of the String object.
        [[nodiscard]]
        virtual const char* bytes() const = 0;

        // Returns the backing byte array, and releases ownership of it. The caller is now responsible for freeing the
        // memory.
        virtual std::unique_ptr<const char[]> release_bytes() = 0;

        // The length of the string in bytes, excluding the terminating `NUL` character.
        [[nodiscard]]
        virtual size_t length() const = 0;

        // Determines if the string is ASCII-safe or not. Multiple subsequent calls to this method may return a cached
        // result from a previous run. The first call may use a pre-calculated value, but this is not guaranteed by
        // this method.
        [[nodiscard]]
        virtual bool is_ascii() = 0;

        // TODO: Should return perlang::type or similar, like System.Type in .NET
        // Returns the type of the object.
        [[nodiscard]]
        virtual std::unique_ptr<perlang::String> get_type() const = 0;

        // Concatenate this string with another string. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        virtual std::unique_ptr<String> operator+(String& rhs) const = 0;

        // Concatenates this string with an int32. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        virtual std::unique_ptr<String> operator+(int32_t rhs) const
        {
            return this->operator+(static_cast<int64_t>(rhs));
        }

        // Concatenates this string with an int64. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        virtual std::unique_ptr<String> operator+(int64_t rhs) const = 0;

        // Concatenates this string with a uint32. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        virtual std::unique_ptr<String> operator+(uint32_t rhs) const
        {
            return this->operator+(static_cast<uint64_t>(rhs));
        }

        // Concatenates this string with a uint64. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        virtual std::unique_ptr<String> operator+(uint64_t rhs) const = 0;

        // Concatenates this string with a float. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        virtual std::unique_ptr<String> operator+(float rhs) const = 0;

        // Concatenates this string with a double. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        virtual std::unique_ptr<String> operator+(double rhs) const = 0;

        // Concatenates this string with a BigInt. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        virtual std::unique_ptr<String> operator+(const BigInt& rhs) const = 0;

        // Compares this string to another string, returning true if they are equal.
        [[nodiscard]]
        inline bool operator==(const String& rhs)
        {
            return *this == const_cast<String&>(rhs);
        }

        // Compares this string to another string, returning true if they are equal.
        [[nodiscard]]
        inline bool operator==(String& rhs)
        {
            return *this == &rhs;
        }

        // Compares this string to another string, returning true if they are equal.
        [[nodiscard]]
        bool operator==(UTF8String& rhs);

        // Compares this string to another string, returning true if they are equal.
        [[nodiscard]]
        bool operator==(String* rhs);

        // Compares this string to another string, returning true if they are not equal.
        [[nodiscard]]
        bool operator!=(const String& rhs);

        // Compares this string to another string, returning true if they are not equal.
        [[nodiscard]]
        bool operator!=(String& rhs);

        // Compares this string to another string, returning true if they are not equal.
        [[nodiscard]]
        bool operator!=(UTF8String& rhs);

        // Compares this string to another string, returning true if they are not equal.
        [[nodiscard]]
        bool operator!=(String* rhs);
    };
}
