#include "bigint.hpp"

namespace perlang
{
    BigInt BigInt_mod(const BigInt& value, const BigInt& exponent)
    {
        return value % exponent;
    }
}