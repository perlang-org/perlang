#include <cstdint>

#pragma once

namespace perlang
{
    class CharRange
    {
     private:
        char16_t begin_;
        char16_t end_;

     public:
        static CharRange from(char16_t begin, char16_t end)
        {
            return CharRange(begin, end);
        }

        CharRange(char16_t begin, char16_t end)
        {
            begin_ = begin;
            end_ = end;
        }

        [[nodiscard]]
        bool contains(char16_t i) const
        {
            return i >= begin_ &&
                   i <= end_;
        }
    };
}
