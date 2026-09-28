#pragma once

#include <vector>

#include "perlang_string.h"

namespace perlang::text
{
    class UTF16StringBuilder
    {
     private:
        std::vector<uint16_t> buffer_;

     public:
        void append(const String& str);

        inline void append(const std::unique_ptr<String>& str)
        {
            append(*str);
        }

        inline void append(const std::unique_ptr<ASCIIString>& str)
        {
            append(*str);
        }

        inline void append(const std::unique_ptr<UTF8String>& str)
        {
            append(*str);
        }

        inline void append(const std::unique_ptr<UTF16String>& str)
        {
            append(*str);
        }

        void append(char16_t c);

        void append_line(const String& str);

        inline void append_line(const std::unique_ptr<String>& str)
        {
            append_line(*str);
        }

        inline void append_line(const std::unique_ptr<ASCIIString>& str)
        {
            append_line(*str);
        }

        inline void append_line(const std::unique_ptr<UTF8String>& str)
        {
            append_line(*str);
        }

        inline void append_line(const std::unique_ptr<UTF16String>& str)
        {
            append_line(*str);
        }

        // The length in number of UTF-16 code units, like UTF16String::length().
        [[nodiscard]]
        uint64_t length() const;

        [[nodiscard]]
        std::unique_ptr<String> to_string() const;
    };
}

namespace perlang
{
    // TODO: Perlang code currently refers to stdlib classes using the 'perlang' namespace, since we don't yet have an
    // import/using statement like in other languages: https://gitlab.perlang.org/perlang/perlang/-/work_items/528
    using UTF16StringBuilder = text::UTF16StringBuilder;
    using StringBuilder = text::UTF16StringBuilder;
}
