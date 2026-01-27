#include <cstdint>

#pragma once

namespace perlang
{
    class ULongRange
    {
     private:
        uint64_t begin_;
        uint64_t end_;

     public:
        static ULongRange from(uint64_t begin, uint64_t end)
        {
            return ULongRange(begin, end);
        }

        ULongRange(uint64_t begin, uint64_t end)
        {
            begin_ = begin;
            end_ = end;
        }

        [[nodiscard]]
        bool contains(uint64_t i) const
        {
            return i >= begin_ &&
                   i <= end_;
        }
    };
}
