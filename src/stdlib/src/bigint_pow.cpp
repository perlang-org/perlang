#include "bigint.hpp"

namespace perlang
{
    BigInt _BigInt_pow(const BigInt& value_to_return, const BigInt& value, int exponent);

    BigInt BigInt_pow(const BigInt& value, int exponent)
    {
        // "With constant auxiliary memory" implementation as given in
        // https://en.wikipedia.org/wiki/Exponentiation_by_squaring
        return _BigInt_pow(1, value, exponent);
    }

    // Private helper method for the above
    BigInt _BigInt_pow(const BigInt& value_to_return, const BigInt& value, int exponent)
    {
        if (exponent < 0) {
            return _BigInt_pow(value_to_return, 1 / value, -exponent);
        }
        else if (exponent == 0) {
            return value_to_return;
        }
        else if (exponent % 2 == 0) {
            return _BigInt_pow(value_to_return, value * value, exponent / 2);
        }
        else { // exponent is odd
            return _BigInt_pow(value_to_return * value, value * value, (exponent - 1) / 2);
        }
    }
}