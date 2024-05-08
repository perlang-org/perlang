#pragma once

// Forward declaration to avoid circular dependencies
class BigInt;

namespace perlang
{
    // Abstract base class for all String types in Perlang
    class String
    {
     public:
        // Returns the backing byte array for this String. This method is generally to be avoided; it is safer to use
        // the String throughout the code and only call this when you really must.
        [[nodiscard]]
        virtual const char* bytes() const = 0;

        // The length of the string in bytes, excluding the terminating `NUL` character.
        [[nodiscard]]
        virtual size_t length() const = 0;

        // Concatenate this string with another string. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        virtual std::shared_ptr<const String> operator+(const String& rhs) const = 0;

        // Concatenates this string with an int32. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        virtual std::shared_ptr<const String> operator+(int32_t rhs) const
        {
            return this->operator+(static_cast<int64_t>(rhs));
        }

        // Concatenates this string with an int64. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        virtual std::shared_ptr<const String> operator+(int64_t rhs) const = 0;

        // Concatenates this string with a uint32. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        virtual std::shared_ptr<const String> operator+(uint32_t rhs) const
        {
            return this->operator+(static_cast<uint64_t>(rhs));
        }

        // Concatenates this string with a uint64. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        virtual std::shared_ptr<const String> operator+(uint64_t rhs) const = 0;

        // Concatenates this string with a BigInt. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        virtual std::shared_ptr<const String> operator+(const BigInt& rhs) const = 0;
    };
}
