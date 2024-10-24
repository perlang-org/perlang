#pragma once

// Forward declaration to avoid circular dependencies
class BigInt;

namespace perlang
{
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

        // Returns the backing byte array, and release ownership of it. The caller is now responsible for freeing the
        // memory.
        virtual std::unique_ptr<const char[]> release_bytes() = 0;

        // The length of the string in bytes, excluding the terminating `NUL` character.
        [[nodiscard]]
        virtual size_t length() const = 0;

        // Concatenate this string with another string. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        virtual std::unique_ptr<String> operator+(const String& rhs) const = 0;

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
    };
}
