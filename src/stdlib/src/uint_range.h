#include <cstdint>

#pragma once

namespace perlang
{
    class UIntRange
    {
     private:
        uint32_t begin_;
        uint32_t end_;

     public:
        static UIntRange from(uint32_t begin, uint32_t end)
        {
            return UIntRange(begin, end);
        }

        UIntRange(uint32_t begin, uint32_t end)
        {
            begin_ = begin;
            end_ = end;
        }

        [[nodiscard]]
        bool contains(uint32_t i) const
        {
            return i >= begin_ &&
                   i <= end_;
        }
    };
}
