#pragma once

#include <stdint.h>

#include "bigint.hpp"

namespace perlang
{
    namespace stdlib
    {
        class Base64
        {
        public:
            static const char *to_string();
        };
    }

    void print(const char *str);
    void print(bool b);
    void print(char c);
    void print(int32_t i);
    void print(uint32_t u);
    void print(int64_t i);
    void print(uint64_t i);
    void print(const BigInt& bigint);
    void print(float f);
    void print(double d);
}
