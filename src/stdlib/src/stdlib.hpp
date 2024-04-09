#pragma once

#include <memory> // std::shared_ptr
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
            static std::shared_ptr<const ASCIIString> to_string();
        };
    }

    // C++ doesn't seem to have the kind of covariance we intend for Perlang. This means that we have to define these
    // for all the existing String types instead of just receiving `const String`-type parameters.
    void print(const String* str);
    void print(const ASCIIString& str);
    void print(const std::shared_ptr<const String>& str);
    void print(const std::shared_ptr<const ASCIIString>& str);

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
