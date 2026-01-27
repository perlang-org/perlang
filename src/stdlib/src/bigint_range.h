#include <cstdint>

#pragma once

namespace perlang
{
    class BigIntRange
    {
     private:
        BigInt begin_;
        BigInt end_;

     public:
        static BigIntRange from(BigInt begin, BigInt end)
        {
            return BigIntRange(begin, end);
        }

        BigIntRange(BigInt begin, BigInt end)
        {
            begin_ = begin;
            end_ = end;
        }

        [[nodiscard]]
        bool contains(BigInt i) const
        {
            return i >= begin_ &&
                   i <= end_;
        }
    };
}
