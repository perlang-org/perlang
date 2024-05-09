#include "bigint.h"
#include "perlang_stdlib.h"

namespace perlang
{
    // Helper method for performing BigInt-based exponentiation. This makes it easy from the calling side to use this
    // regardless of whether the left-hand operand is int32, uint32, int64, uint16 or BigInt; they can all implicitly be
    // converted to BigInt.
    BigInt BigInt_pow(const BigInt& value, int exponent)
    {
        return value.pow(exponent);
    }
}
