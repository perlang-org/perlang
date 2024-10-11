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
        [[nodiscard]]
        static std::unique_ptr<const UTF8String> from_static_string(const char* s);

        // Creates a new UTF8String from an "owned string", like a string that has been allocated on the heap. The
        // ownership of the memory is transferred to the UTF8String, which is then responsible for deallocating the
        // memory when it is no longer needed (i.e. when no references to it remains).
        [[nodiscard]]
        static std::unique_ptr<const UTF8String> from_owned_string(const char* s, size_t length);

        // Creates a new UTF8String from an existing string, by copying its content to a new buffer allocated on the
        // heap. The UTF8String class takes ownership of the newly allocated buffer, which will be deallocated when the
        // UTF8String runs out of scope.
        [[nodiscard]]
        static std::unique_ptr<const UTF8String> from_copied_string(const char* str);

     private:
        // Private constructor for creating a new UTF8String from a C-style (NUL-terminated) string. The `owned`
        // parameter indicates whether the UTF8String should take ownership of the memory it points to, and thus be
        // responsible for deallocating it when it is no longer needed.
        UTF8String(const char* string, size_t length, bool owned);

     public:
        virtual ~UTF8String();

        // Returns the backing byte array for this UTF8String. This method is generally to be avoided; it is safer to
        // use the UTF8String throughout the code and only call this when you really must.
        [[nodiscard]]
        const char* bytes() const override;

        // The length of the string in bytes, excluding the terminating `NUL` character.
        [[nodiscard]]
        size_t length() const override;

        // Compares the equality of two `UTF8String`s, returning `true` if they point to the same backing byte array
        // and have the same length. Note that this method does *not* compare referential equality; two different
        // `UTF8String` instances pointing at the same backing byte array are considered equal in Perlang. For all
        // practical matters, they can be considered interchangeable because strings are guaranteed to be immutable for
        // the whole lifetime of the program.
        bool operator==(const UTF8String& rhs) const;

        // Compares the equality of two `UTF8String`s, returning `true` if they are non-equal. See the `operator==`
        // documentation for more semantic details about the implementation.
        bool operator!=(const UTF8String& rhs) const;

        // Concatenate this string with another string. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        std::unique_ptr<const String> operator+(const String& rhs) const override;

        // Concatenates this string with another string. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        std::unique_ptr<const UTF8String> operator+(const UTF8String& rhs) const;

        // Concatenates this string with an int or long. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        std::unique_ptr<const String> operator+(int64_t rhs) const override;

        // Concatenates this string with an int or long. The memory for the new string is allocated from the heap.
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

        // Concatenates this string with a std::string. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        inline std::unique_ptr<const String> operator+(const std::string& rhs) const;

     private:
        // The backing byte array for this string. This is to be considered immutable and MUST NOT be modified at any
        // point. There might be multiple UTF8String objects pointing to the same `bytes_`, so modifying one of them
        // would unintentionally spread the modifications to these other objects too.
        const char* bytes_;

        // The length of the string in bytes, excluding the terminating `NUL` character.
        size_t length_;

        // A flag indicating whether this string owns the memory it points to. If this is true, the string is
        // responsible for deallocating the memory when it is no longer needed. If false, the string is borrowing the
        // memory from somewhere else, and should not deallocate it.
        bool owned_;
    };

    // Concatenate an int/long+UTF8String. The memory for the new string is allocated from the heap. This is a free
    // function, since the left-hand side is not an UTF8String.
    [[nodiscard]]
    std::unique_ptr<const UTF8String> operator+(int64_t lhs, const UTF8String& rhs);

    // Concatenate a uint/ulong+UTF8String. The memory for the new string is allocated from the heap. This is a free
    // function, since the left-hand side is not an UTF8String.
    [[nodiscard]]
    std::unique_ptr<const UTF8String> operator+(uint64_t lhs, const UTF8String& rhs);

    // TODO: missing + operator for int32_t and uint32_t

    // Concatenate a float+UTF8String. The memory for the new string is allocated from the heap. This is a free
    // function, since the left-hand side is not an UTF8String.
    [[nodiscard]]
    std::unique_ptr<const UTF8String> operator+(float lhs, const UTF8String& rhs);

    // Concatenate a double+UTF8String. The memory for the new string is allocated from the heap. This is a free
    // function, since the left-hand side is not an UTF8String.
    [[nodiscard]]
    std::unique_ptr<const UTF8String> operator+(double lhs, const UTF8String& rhs);

    // Concatenate a double+UTF8String. The memory for the new string is allocated from the heap. This is a free
    // function, since the left-hand side is not an UTF8String.
    [[nodiscard]]
    inline std::unique_ptr<const UTF8String> operator+(const std::string& lhs, const UTF8String& rhs);
}
