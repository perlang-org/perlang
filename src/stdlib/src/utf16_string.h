#pragma once

#include <vector>

#include "perlang_string.h"

namespace perlang
{
    class UTF16String : public String
    {
     public:
        // Creates a new UTF16String from an "owned string", which is presumed to have been allocated on the heap using
        // the C++ "new" operator. The ownership of the memory is transferred to the UTF8String, which is then
        // responsible for deallocating the memory when it is no longer needed (i.e. when no references to it remains).
        [[nodiscard]]
        static std::unique_ptr<UTF16String> from_owned_string(std::vector<uint16_t> s);

        // Creates a new UTF8String from an existing string, by copying its content to a new buffer allocated on the
        // heap. The UTF16String class takes ownership of the newly allocated buffer, which will be deallocated when the
        // UTF16String runs out of scope.
        [[nodiscard]]
        static std::unique_ptr<UTF16String> from_copied_string(const char* str);

        // Creates a new UTF16String from an existing string, by copying its content to a new buffer allocated on the
        // heap. The UTF16String class takes ownership of the newly allocated buffer, which will be deallocated when the
        // UTF16String runs out of scope.
        [[nodiscard]]
        static std::unique_ptr<UTF16String> from_copied_string(const char* str, size_t length);

     private:
        // Private constructor for creating a new UTF16String from an array of UTF16LE bytes.
        explicit UTF16String(std::vector<uint16_t> string);

     public:
        ~UTF16String() override;

        // Returns the backing byte array for this UTF816String. This method is generally to be avoided; it is safer to
        // use the UTF16String throughout the code and only call this when you really must.
        [[nodiscard]]
        const char* bytes() const override;

        // Returns the backing byte array, and releases ownership of it. The caller is now responsible for freeing the
        // memory.
        std::unique_ptr<const char[]> release_bytes() override;

        // The length of the string in number of uint16 elements
        [[nodiscard]]
        size_t length() const override;

        // Determines if the string is ASCII safe or not. Multiple subsequent calls to this method may return a cached
        // result from a previous run. The first call may use a pre-calculated value, but this is not guaranteed by
        // this method.
        bool is_ascii() override;

        [[nodiscard]]
        std::unique_ptr<UTF16String> copy() const
        {
            std::vector<uint16_t> copied_data = data_;
            auto new_string = new UTF16String(std::move(copied_data));
            return std::unique_ptr<UTF16String>(new_string);
        }

        [[nodiscard]]
        std::unique_ptr<String> get_type() const override
        {
            return ASCIIString::from_static_string("perlang::UTF16String");
        }

        [[nodiscard]]
        std::unique_ptr<UTF16String> as_utf16() const override;

        // Indexes the string, returning the character at the given position. Note that this method performs bounds
        // checking; attempting to read outside the string will result in an exception.
        char16_t operator[](size_t index) const;

        // Concatenates this string with another string. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        std::unique_ptr<String> operator+(String& rhs) const override;

        // Concatenates this string with an int or long. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        std::unique_ptr<String> operator+(int64_t rhs) const override;

        // Concatenates this string with an int or long. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        std::unique_ptr<String> operator+(uint64_t rhs) const override;

        // Concatenates this string with a float. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        std::unique_ptr<String> operator+(float rhs) const override;

        // Concatenates this string with a double. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        std::unique_ptr<String> operator+(double rhs) const override;

        // Concatenates this string with a BigInt. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        std::unique_ptr<String> operator+(const BigInt& rhs) const override;

        // Concatenates this string with a std::string. The memory for the new string is allocated from the heap.
        [[nodiscard]]
        inline std::unique_ptr<String> operator+(const std::string& rhs) const;

     private:
        // The backing UTF16LE array for this string. This is to be considered immutable and MUST NOT be modified at any
        // point. There might be multiple UTF16String objects pointing to the same `data_`, so modifying one of them
        // would unintentionally spread the modifications to these other objects too.
        std::vector<uint16_t> data_;

        // A flag indicating whether this string contains only ASCII characters or not. Strings containing ASCII-only
        // content can be converted to ASCIIString without errors.
        std::unique_ptr<bool> is_ascii_;
    };
}
