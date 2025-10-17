#pragma once

// This is the Perlang standard library entrypoint, which gets included by the Perlang-generated C++ code. All the
// public API of the Perlang standard library should be defined here, either directly or indirectly (by #including other
// header files) so that the generated code will find all the necessary definitions.

#include <memory> // std::shared_ptr
#include <cstdint>

#include "ascii_string.h"
#include "bigint.h"
#include "int_array.h"
#include "object_array.h"
#include "perlang_char.h"
#include "perlang_string.h"
#include "perlang_type.h"
#include "perlang_value_types.h"
#include "string_array.h"
#include "utf8_string.h"
#include "utf16_string.h"

#include "collections/mutable_string_hash_set.h"
#include "collections/string_hash_set.h"

#include "exceptions/null_pointer_exception.h"
#include "exceptions/illegal_state_exception.h"

#include "io/file.h"

#include "text/string_builder.h"

// TODO: Extract to separate header files instead of keeping it in a single file

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

    // Overload for pointer arguments
    void print(const Object* obj);
    void print(const String* str);
    void print(const UTF16String* str);
    void print(const ASCIIString& str);

    // unique_ptr overloads
    void print(const std::unique_ptr<Object>& obj);
    void print(const std::unique_ptr<const Object>& obj);
    void print(const std::unique_ptr<String>& str);
    void print(const std::unique_ptr<const String>& str);
    void print(const std::unique_ptr<ASCIIString>& str);
    void print(const std::unique_ptr<const ASCIIString>& str);
    void print(const std::unique_ptr<UTF8String>& str);
    void print(const std::unique_ptr<const UTF8String>& str);
    void print(const std::unique_ptr<UTF16String>& str);
    void print(const std::unique_ptr<const UTF16String>& str);

    // shared_ptr overloads
    void print(const std::shared_ptr<Object>& obj);
    void print(const std::shared_ptr<const Object>& obj);
    void print(const std::shared_ptr<String>& str);
    void print(const std::shared_ptr<const String>& str);
    void print(const std::shared_ptr<ASCIIString>& str);
    void print(const std::shared_ptr<const ASCIIString>& str);
    void print(const std::shared_ptr<UTF8String>& str);
    void print(const std::shared_ptr<const UTF8String>& str);
    void print(const std::shared_ptr<UTF16String>& str);
    void print(const std::shared_ptr<const UTF16String>& str);

    // Overloads for built-in value types
    void print(bool b);
    void print(char c);
    void print(char16_t c);
    void print(int32_t i);
    void print(uint32_t u);
    void print(int64_t i);
    void print(uint64_t i);
    void print(const BigInt& bigint);
    void print(float f);
    void print(double d);

    // TODO: If we would let all reference types inherit from PerlangObject, and use a to_string() approach like in
    // C#/.NET, we could probably get rid of this (as well as the String/ASCIIString/etc overloads above).
    // https://gitlab.perlang.org/perlang/perlang/-/issues/251
    void print(std::shared_ptr<PerlangType> type);

    // TODO: We would like to enable this, but it breaks with EnumTests since they expect to be able to implicitly
    // TODO: convert enum values to int32_t.
    // Ensure that print() is never called with any other argument than the ones explicitly defined above.
    //template <class T>
    //void print(T) = delete;

    BigInt BigInt_mod(const BigInt& value, const BigInt& divisor);
    BigInt BigInt_pow(const BigInt& value, int exponent);
}
