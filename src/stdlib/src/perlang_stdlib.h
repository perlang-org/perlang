#pragma once

// This is the Perlang standard library entrypoint, which gets included by the Perlang-generated C++ code. All the
// public API of the Perlang standard library should be defined here, either directly or indirectly (by #including other
// header files) so that the generated code will find all the necessary definitions.

#include <memory> // std::shared_ptr
#include <cstdint>

#include "ascii_string.h"
#include "bigint.h"
#include "int_array.h"
#include "perlang_string.h"
#include "string_array.h"
#include "utf8_string.h"

#include "collections/mutable_string_hash_set.h"
#include "collections/string_hash_set.h"

#include "io/file.h"

#include "text/string_builder.h"

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
    void print(const std::unique_ptr<String>& str);
    void print(const std::unique_ptr<const String>& str);
    void print(const std::unique_ptr<ASCIIString>& str);
    void print(const std::unique_ptr<const ASCIIString>& str);
    void print(const std::unique_ptr<UTF8String>& str);
    void print(const std::unique_ptr<const UTF8String>& str);
    void print(const std::shared_ptr<const String>& str);
    void print(const std::shared_ptr<const UTF8String>& str);

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
