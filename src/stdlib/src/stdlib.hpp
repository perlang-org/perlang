#pragma once

#include <stdint.h>

#include "ascii_string.h"
#include "bigint.hpp"
#include "perlang_string.h"
#include "utf8_string.h"

namespace perlang
{
    namespace stdlib
    {
        class Base64
        {
         public:
            static ASCIIString to_string();
        };
    }

    void print(const String& str);
    void print(bool b);
    void print(char c);
    void print(int32_t i);
    void print(uint32_t u);
    void print(int64_t i);
    void print(uint64_t i);
    void print(const BigInt& bigint);
    void print(float f);
    void print(double d);

    BigInt BigInt_mod(const BigInt& value, const BigInt& divisor);
    BigInt BigInt_pow(const BigInt& value, int exponent);
}
