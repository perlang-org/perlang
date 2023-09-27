#ifndef PERLANG_STDLIB_HPP
#define PERLANG_STDLIB_HPP

#include <stdint.h>

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
    void print(uint64_t u);
    void print(float f);
    void print(double d);
}

#endif //PERLANG_STDLIB_HPP
