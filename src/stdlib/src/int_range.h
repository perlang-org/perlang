#include <cstdint>

#pragma once

namespace perlang
{
    class IntRange
    {
     private:
        int32_t begin_;
        int32_t end_;

     public:
        static IntRange from(int32_t begin, int32_t end)
        {
            return IntRange(begin, end);
        }

        IntRange(int32_t begin, int32_t end)
        {
            begin_ = begin;
            end_ = end;
        }

        [[nodiscard]]
        bool contains(int32_t i) const
        {
            return i >= begin_ &&
                   i <= end_;
        }
    };
}
