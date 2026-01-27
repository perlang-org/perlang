#include <cstdint>

#pragma once

namespace perlang
{
    class LongRange
    {
     private:
        int64_t begin_;
        int64_t end_;

     public:
        static LongRange from(int64_t begin, int64_t end)
        {
            return LongRange(begin, end);
        }

        LongRange(int64_t begin, int64_t end)
        {
            begin_ = begin;
            end_ = end;
        }

        [[nodiscard]]
        bool contains(int64_t i) const
        {
            return i >= begin_ &&
                   i <= end_;
        }
    };
}
