/*
    BigInt
    ------
    Arbitrary-sized integer class for C++ (rewritten to use libtommath as backend)

    Based on C++ API by Syed Faheel Ahmad (faheel@live.in), in https://github.com/faheel/BigInt
    Licensed under the MIT (Expat) license.

    The libtommath-wrapper is inspired by the boost/multiprecision/tommath.hpp file, licensed under the following terms:

    Copyright 2011 John Maddock.
    Copyright 2021 Matt Borland. Distributed under the Boost
    Software License, Version 1.0. (See accompanying file
    LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
*/

// Extracted from bigint.hpp, to avoid "multiple definition" errors when linking.

#include "bigint.hpp"

#include <iostream>
#include <cmath>

/*
    ===========================================================================
    Utility functions
    ===========================================================================
*/

#ifndef BIG_INT_UTILITY_FUNCTIONS_HPP
#define BIG_INT_UTILITY_FUNCTIONS_HPP

#include <tuple>
#include <cstring>
#include <memory>

inline void check_tommath_result(mp_err v)
{
    if (v != MP_OKAY) {
        throw std::runtime_error(mp_error_to_string(v));
    }
}

#endif  // BIG_INT_UTILITY_FUNCTIONS_HPP


/*
    ===========================================================================
    Random number generating functions for BigInt
    ===========================================================================
*/

/*
    ===========================================================================
    Constructors
    ===========================================================================
*/

#ifndef BIG_INT_CONSTRUCTORS_HPP
#define BIG_INT_CONSTRUCTORS_HPP

/*
    Default constructor
    -------------------
*/

BigInt::BigInt()
{
    check_tommath_result(mp_init(&data));
}

/*
    Copy constructor
    ----------------
*/

BigInt::BigInt(const BigInt& num)
{
    check_tommath_result(mp_init_copy(&data, &num.data));
}

/*
    Integer (various sizes) to BigInt
    -----------------
*/

BigInt::BigInt(const int& num) :
    BigInt((long long)num)
{
}

BigInt::BigInt(const unsigned int& num) :
    BigInt((unsigned long long)num)
{
}

BigInt::BigInt(const long& num) :
    BigInt((long long)num)
{
}

BigInt::BigInt(const unsigned long& num) :
    BigInt((unsigned long long)num)
{
}

BigInt::BigInt(const long long& num)
{
    check_tommath_result(mp_init(&data));

    *this = num;
}

BigInt::BigInt(const unsigned long long& num)
{
    check_tommath_result(mp_init(&data));

    *this = num;
}

BigInt::BigInt(const double& num)
{
    if (ceil(num) != num) {
        // num has a fractional part and can inherently never be equal to a BigInt.
        throw std::invalid_argument(
            "Expected a value without any fractional part, got \'" + std::to_string(num) + "\'");
    }

    check_tommath_result(mp_init(&data));

    *this = (long)num;
}

/*
    String to BigInt
    ----------------
*/

BigInt::BigInt(const char* s)
{
    //
    // We don't use libtommath's own routine because it doesn't error check the input :-( This logic, like much of the
    // rest of the file, is borrowed straight from the Boost implementation (from `tommath_int& operator=(const char* s)`
    // in this specific case)
    //
    check_tommath_result(mp_init(&data));

    std::size_t n = s ? strlen(s) : 0;
    *this = static_cast<std::uint32_t>(0u);
    unsigned radix = 10;
    bool isneg = false;

    if (n && (*s == '-')) {
        --n;
        ++s;
        isneg = true;
    }

    if (n && (*s == '0')) {
        if ((n > 1) && ((s[1] == 'x') || (s[1] == 'X'))) {
            radix = 16;
            s += 2;
            n -= 2;
        }
        else {
            radix = 8;
            n -= 1;
        }
    }

    if (n) {
        if (radix == 8 || radix == 16) {
            unsigned shift = radix == 8 ? 3 : 4;
#ifndef MP_DIGIT_BIT
            unsigned block_count = DIGIT_BIT / shift;
#else
            unsigned block_count = MP_DIGIT_BIT / shift;
#endif
            unsigned block_shift = shift * block_count;
            unsigned long long val, block;
            while (*s) {
                block = 0;
                for (unsigned i = 0; (i < block_count); ++i) {
                    if (*s >= '0' && *s <= '9') {
                        val = *s - '0';
                    }
                    else if (*s >= 'a' && *s <= 'f') {
                        val = 10 + *s - 'a';
                    }
                    else if (*s >= 'A' && *s <= 'F') {
                        val = 10 + *s - 'A';
                    }
                    else {
                        val = 400;
                    }

                    if (val > radix) {
                        throw std::runtime_error("Unexpected content found while parsing character string.");
                    }

                    block <<= shift;
                    block |= val;

                    if (!*++s) {
                        // final shift is different:
                        block_shift = (i + 1) * shift;
                        break;
                    }
                }

                check_tommath_result(mp_mul_2d(&get_data(), block_shift, &get_data()));

                if (data.used) {
                    data.dp[0] |= block;
                }
                else {
                    *this = block;
                }
            }
        }
        else {
            // Base 10, we extract blocks of size 10^9 at a time, that way
            // the number of multiplications is kept to a minimum:
            std::uint32_t block_mult = 1000000000;

            while (*s) {
                std::uint32_t block = 0;

                for (unsigned i = 0; i < 9; ++i) {
                    std::uint32_t val;
                    if (*s >= '0' && *s <= '9') {
                        val = *s - '0';
                    }
                    else {
                        throw std::runtime_error("Unexpected character encountered in input.");
                    }

                    block *= 10;
                    block += val;

                    if (!*++s) {
                        constexpr const std::uint32_t block_multiplier[9] = { 10, 100, 1000, 10000, 100000, 1000000,
                                                                              10000000, 100000000, 1000000000 };
                        block_mult = block_multiplier[i];
                        break;
                    }
                }

                BigInt t;
                t = block_mult;
                eval_multiply(*this, t);
                t = block;
                eval_add(*this, t);
            }
        }
    }

    if (isneg) {
        check_tommath_result(mp_neg(&data, &data));
    }
}

BigInt::~BigInt()
{
    if (data.dp != nullptr) {
        mp_clear(&data);
    }
}

#endif  // BIG_INT_CONSTRUCTORS_HPP


/*
    ===========================================================================
    Conversion functions for BigInt
    ===========================================================================
*/

#ifndef BIG_INT_CONVERSION_FUNCTIONS_HPP
#define BIG_INT_CONVERSION_FUNCTIONS_HPP

/*
    to_string
    ---------
    Converts a BigInt to a string.
*/

std::string BigInt::to_string() const
{
    // Only support base-10 for now. The Boost multiprecision/tommath.hpp file supports more, so if we ever need that,
    // we can look in that file. This method is a simplified version of the str() method there; supporting hex can
    // require a new copy-paste from there (to support upper/lowercasing etc).
    const int base = 10;
    int s;
    check_tommath_result(mp_radix_size(const_cast< ::mp_int*>(&data), base, &s));
    std::unique_ptr<char[]> a(new char[s + 1]);
#ifndef mp_to_binary
    detail::check_tommath_result(mp_toradix_n(const_cast< ::mp_int*>(&m_data), a.get(), base, s + 1));
#else
    std::size_t written;
    check_tommath_result(mp_to_radix(&data, a.get(), s + 1, &written, base));
#endif
    std::string result = a.get();

    return result;
}

#endif  // BIG_INT_CONVERSION_FUNCTIONS_HPP


/*
    ===========================================================================
    Assignment operators
    ===========================================================================
*/

#ifndef BIG_INT_ASSIGNMENT_OPERATORS_HPP
#define BIG_INT_ASSIGNMENT_OPERATORS_HPP

/*
    BigInt = BigInt
    ---------------
*/

BigInt& BigInt::operator=(const BigInt& num)
{
    if (data.dp == nullptr) {
        check_tommath_result(mp_init(&data));
    }

    if (num.data.dp != nullptr) {
        check_tommath_result(mp_copy(const_cast< ::mp_int*>(&num.data), &data));
    }

    return *this;
}

/*
    BigInt = Integer
    ----------------
*/

BigInt& BigInt::operator=(const int& num)
{
    return *this = (long long)num;
}

BigInt& BigInt::operator=(const unsigned int& num)
{
    return *this = (unsigned long long)num;
}

BigInt& BigInt::operator=(const long& num)
{
    return *this = (long long)num;
}

BigInt& BigInt::operator=(const long long& num)
{
    if (data.dp == nullptr) {
        check_tommath_result(mp_init(&data) != MP_OKAY);
    }

    bool neg = num < 0;
    *this = (unsigned long long)llabs(num);

    if (neg) {
        check_tommath_result(mp_neg(&data, &data) != MP_OKAY);
    }

    return *this;
}

BigInt& BigInt::operator=(const unsigned long long& num)
{
    if (data.dp == nullptr) {
        check_tommath_result(mp_init(&data));
    }

    mp_set_u64(&data, num);

    return *this;
}

/*
    BigInt = String
    ---------------
*/

BigInt& BigInt::operator=(const char* s)
{
    // Create a temporary instance to use the BigInt(const char*) constructor for parsing the string
    BigInt temp(s);
    data = temp.data;

    return *this;
}

#endif  // BIG_INT_ASSIGNMENT_OPERATORS_HPP


/*
    ===========================================================================
    Unary arithmetic operators
    ===========================================================================
*/

#ifndef BIG_INT_UNARY_ARITHMETIC_OPERATORS_HPP
#define BIG_INT_UNARY_ARITHMETIC_OPERATORS_HPP

/*
    +BigInt
    -------
    Returns the value of a BigInt.
    NOTE: This function does not return the absolute value. To get the absolute
    value of a BigInt, use the `abs` function.
*/

BigInt BigInt::operator+() const
{
    return *this;
}

/*
    -BigInt
    -------
    Returns the negative of a BigInt.
*/

BigInt BigInt::operator-() const
{
    BigInt result;

    check_tommath_result(mp_neg(&data, &result.data));
    return result;
}

#endif  // BIG_INT_UNARY_ARITHMETIC_OPERATORS_HPP


/*
    ===========================================================================
    Relational operators
    ===========================================================================
    All operators depend on the '<' and/or '==' operator(s).
*/

#ifndef BIG_INT_RELATIONAL_OPERATORS_HPP
#define BIG_INT_RELATIONAL_OPERATORS_HPP

/*
    BigInt == BigInt
    ----------------
*/

bool BigInt::operator==(const BigInt& num) const
{
    return mp_cmp(const_cast< ::mp_int*>(&data), const_cast< ::mp_int*>(&num.data)) == MP_EQ;
}

/*
    BigInt != BigInt
    ----------------
*/

bool BigInt::operator!=(const BigInt& num) const
{
    return !(*this == num);
}

/*
    BigInt < BigInt
    ---------------
*/

bool BigInt::operator<(const BigInt& num) const
{
    return mp_cmp(const_cast< ::mp_int*>(&data), const_cast< ::mp_int*>(&num.data)) == MP_LT;
}

/*
    BigInt > BigInt
    ---------------
*/

bool BigInt::operator>(const BigInt& num) const
{
    return mp_cmp(const_cast< ::mp_int*>(&data), const_cast< ::mp_int*>(&num.data)) == MP_GT;
}

/*
    BigInt <= BigInt
    ----------------
*/

bool BigInt::operator<=(const BigInt& num) const
{
    return (*this < num) or (*this == num);
}

/*
    BigInt >= BigInt
    ----------------
*/

bool BigInt::operator>=(const BigInt& num) const
{
    return !(*this < num);
}

/*
    BigInt == Integer
    -----------------
*/

bool BigInt::operator==(const int& num) const
{
    return *this == BigInt(num);
}

bool BigInt::operator==(const unsigned int& num) const
{
    return *this == BigInt(num);
}

bool BigInt::operator==(const long& num) const
{
    return *this == BigInt(num);
}

bool BigInt::operator==(const unsigned long& num) const
{
    return *this == BigInt(num);
}

bool BigInt::operator==(const long long& num) const
{
    return *this == BigInt(num);
}

bool BigInt::operator==(const unsigned long long& num) const
{
    return *this == BigInt(num);
}

bool BigInt::operator==(const double& num) const
{
    if (ceil(num) != num) {
        // num has a fractional part and can inherently never be equal to a BigInt.
        return false;
    }
    else {
        // num does not have a fractional part. Disregarding IEEE754 semantics, we can compare it to a BigInt (reliably
        // for numbers between -2^53 and +2^53, because of the aforementioned IEEE754 semantics).
        return *this == BigInt((int64_t)num);
    }
}

/*
    Integer == BigInt
    -----------------
*/

bool operator==(const int& lhs, const BigInt& rhs)
{
    return BigInt(lhs) == rhs;
}

bool operator==(const unsigned int& lhs, const BigInt& rhs)
{
    return BigInt(lhs) == rhs;
}

bool operator==(const long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) == rhs;
}

bool operator==(const unsigned long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) == rhs;
}

bool operator==(const long long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) == rhs;
}

bool operator==(const unsigned long long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) == rhs;
}

bool operator==(const double& lhs, const BigInt& rhs)
{
    // Delegate to the already defined operator for BigInt==double comparisons
    return rhs == lhs;
}

/*
    BigInt != Integer
    -----------------
*/

bool BigInt::operator!=(const int& num) const
{
    return !(*this == BigInt(num));
}

bool BigInt::operator!=(const unsigned int& num) const
{
    return !(*this == BigInt(num));
}

bool BigInt::operator!=(const long& num) const
{
    return !(*this == BigInt(num));
}
bool BigInt::operator!=(const unsigned long& num) const
{
    return !(*this == BigInt(num));
}

bool BigInt::operator!=(const long long& num) const
{
    return !(*this == BigInt(num));
}

bool BigInt::operator!=(const unsigned long long& num) const
{
    return !(*this == BigInt(num));
}

bool BigInt::operator!=(const double& num) const
{
    if (ceil(num) != num) {
        // num has a fractional part and can inherently never be equal to a BigInt.
        return true;
    }
    else {
        // num does not have a fractional part. Disregarding IEEE754 semantics, we can compare it to a BigInt (reliably
        // for numbers between -2^53 and +2^53, because of the aforementioned IEEE754 semantics).
        return *this != BigInt((int64_t)num);
    }
}

/*
    Integer != BigInt
    -----------------
*/

bool operator!=(const int& lhs, const BigInt& rhs)
{
    return BigInt(lhs) != rhs;
}

bool operator!=(const unsigned int& lhs, const BigInt& rhs)
{
    return BigInt(lhs) != rhs;
}

bool operator!=(const long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) != rhs;
}

bool operator!=(const unsigned long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) != rhs;
}

bool operator!=(const long long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) != rhs;
}

bool operator!=(const unsigned long long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) != rhs;
}

bool operator!=(const double& lhs, const BigInt& rhs)
{
    // Delegate to the already defined operator for BigInt!=double comparisons
    return rhs != lhs;
}

/*
    BigInt < Integer
    ----------------
*/

bool BigInt::operator<(const long long& num) const
{
    return *this < BigInt(num);
}

/*
    Integer < BigInt
    ----------------
*/

bool operator<(const long long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) < rhs;
}

/*
    BigInt > Integer
    ----------------
*/

bool BigInt::operator>(const int& num) const
{
    return *this > BigInt(num);
}

bool BigInt::operator>(const unsigned int& num) const
{
    return *this > BigInt(num);
}

bool BigInt::operator>(const long& num) const
{
    return *this > BigInt(num);
}

bool BigInt::operator>(const unsigned long& num) const
{
    return *this > BigInt(num);
}

bool BigInt::operator>(const long long& num) const
{
    return *this > BigInt(num);
}

bool BigInt::operator>(const unsigned long long& num) const
{
    return *this > BigInt(num);
}

/*
    Integer > BigInt
    ----------------
*/

bool operator>(const int& lhs, const BigInt& rhs)
{
    return BigInt(lhs) > rhs;
}

bool operator>(const unsigned int& lhs, const BigInt& rhs)
{
    return BigInt(lhs) > rhs;
}

bool operator>(const long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) > rhs;
}

bool operator>(const unsigned long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) > rhs;
}

bool operator>(const long long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) > rhs;
}

bool operator>(const unsigned long long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) > rhs;
}

/*
    BigInt <= Integer
    -----------------
*/

bool BigInt::operator<=(const int& num) const
{
    return !(*this > BigInt(num));
}

bool BigInt::operator<=(const unsigned int& num) const
{
    return !(*this > BigInt(num));
}

bool BigInt::operator<=(const long& num) const
{
    return !(*this > BigInt(num));
}

bool BigInt::operator<=(const unsigned long& num) const
{
    return !(*this > BigInt(num));
}

bool BigInt::operator<=(const long long& num) const
{
    return !(*this > BigInt(num));
}

bool BigInt::operator<=(const unsigned long long& num) const
{
    return !(*this > BigInt(num));
}

/*
    Integer <= BigInt
    -----------------
*/

bool operator<=(const int& lhs, const BigInt& rhs)
{
    return BigInt(lhs) <= rhs;
}

bool operator<=(const unsigned int& lhs, const BigInt& rhs)
{
    return BigInt(lhs) <= rhs;
}

bool operator<=(const long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) <= rhs;
}

bool operator<=(const unsigned long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) <= rhs;
}

bool operator<=(const long long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) <= rhs;
}

bool operator<=(const unsigned long long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) <= rhs;
}

/*
    BigInt >= Integer
    -----------------
*/

bool BigInt::operator>=(const int& num) const
{
    return !(*this < BigInt(num));
}

bool BigInt::operator>=(const unsigned int& num) const
{
    return !(*this < BigInt(num));
}

bool BigInt::operator>=(const long& num) const
{
    return !(*this < BigInt(num));
}

bool BigInt::operator>=(const unsigned long& num) const
{
    return !(*this < BigInt(num));
}

bool BigInt::operator>=(const long long& num) const
{
    return !(*this < BigInt(num));
}

bool BigInt::operator>=(const unsigned long long& num) const
{
    return !(*this < BigInt(num));
}

/*
    Integer >= BigInt
    -----------------
*/

bool operator>=(const int& lhs, const BigInt& rhs)
{
    return BigInt(lhs) >= rhs;
}

bool operator>=(const unsigned int& lhs, const BigInt& rhs)
{
    return BigInt(lhs) >= rhs;
}

bool operator>=(const long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) >= rhs;
}

bool operator>=(const unsigned long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) >= rhs;
}

bool operator>=(const long long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) >= rhs;
}

bool operator>=(const unsigned long long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) >= rhs;
}

#endif  // BIG_INT_RELATIONAL_OPERATORS_HPP


/*
    ===========================================================================
    Binary arithmetic operators
    ===========================================================================
*/

#ifndef BIG_INT_BINARY_ARITHMETIC_OPERATORS_HPP
#define BIG_INT_BINARY_ARITHMETIC_OPERATORS_HPP

#include <climits>
#include <cmath>
#include <string>

/*
    BigInt + BigInt
    ---------------
    The operand on the RHS of the addition is `num`.
*/

BigInt BigInt::operator+(const BigInt& num) const
{
    BigInt result;
    check_tommath_result(mp_add(&get_data(), const_cast< ::mp_int*>(&num.get_data()), &result.data));

    return result;
}

/*
    BigInt - BigInt
    ---------------
    The operand on the RHS of the subtraction is `num`.
*/

BigInt BigInt::operator-(const BigInt& num) const
{
    BigInt result;
    check_tommath_result(mp_sub(&get_data(), const_cast< ::mp_int*>(&num.get_data()), &result.data));

    return result;
}

/*
    BigInt * BigInt
    ---------------
    Computes the product of two BigInts using Karatsuba's algorithm.
    The operand on the RHS of the product is `num`.
*/

BigInt BigInt::operator*(const BigInt& num) const
{
    // Optimizations for simple cases which need not call into libtommath
    if (*this == 0 or num == 0) {
        return BigInt(0);
    }

    if (*this == 1) {
        return num;
    }

    if (num == 1) {
        return *this;
    }

    BigInt result;
    check_tommath_result(mp_mul(&get_data(), const_cast< ::mp_int*>(&num.get_data()), &result.data));

    return result;
}

/*
    divide
    ------
    Helper function that returns the quotient and remainder on dividing the
    dividend by the divisor, when the divisor is 1 to 10 times the dividend.
*/

std::tuple<BigInt, BigInt> divide(const BigInt& dividend, const BigInt& divisor)
{
    BigInt quotient, remainder, temp;

    temp = divisor;
    quotient = 1;
    while (temp < dividend) {
        quotient++;
        temp += divisor;
    }
    if (temp > dividend) {
        quotient--;
        remainder = dividend - (temp - divisor);
    }

    return std::make_tuple(quotient, remainder);
}

/*
    BigInt / BigInt
    ---------------
    Computes the quotient of two BigInts using the long-division method.
    The operand on the RHS of the division (the divisor) is `num`.
*/

BigInt BigInt::operator/(const BigInt& num) const
{
    // Optimizations for some simple cases
    if (num == 0) {
        throw std::logic_error("Attempted division by zero");
    }

    if (num == 1) {
        return *this;
    }

    if (num == -1) {
        return -(*this);
    }

    BigInt result;
    BigInt ignored;
    check_tommath_result(mp_div(&get_data(), const_cast< ::mp_int*>(&num.get_data()), &result.data, &ignored.data));

    return result;
}

/*
    BigInt % BigInt
    ---------------
    Computes the modulo (remainder on division) of two BigInts.
    The operand on the RHS of the modulo (the divisor) is `num`.
*/

BigInt BigInt::operator%(const BigInt& num) const
{
    if (num == 0) {
        throw std::logic_error("Attempted division by zero");
    }

    bool neg  = eval_get_sign(*this) < 0;
    bool neg2 = eval_get_sign(num) < 0;

    BigInt result;
    check_tommath_result(mp_mod(&get_data(), const_cast< ::mp_int*>(&num.get_data()), &result.data));

    if ((neg != neg2)) {
        result = -result;
        check_tommath_result(mp_add(&result.get_data(), const_cast< ::mp_int*>(&num.get_data()), &result.data));
        result = -result;
    }
    else if (neg && (result == num)) {
        mp_zero(&result.data);
    }

    return result;
}

/*
    BigInt + Integer
    ----------------
*/

BigInt BigInt::operator+(const long long& num) const
{
    return *this + BigInt(num);
}

/*
    Integer + BigInt
    ----------------
*/

BigInt operator+(const long long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) + rhs;
}

/*
    BigInt - Integer
    ----------------
*/

BigInt BigInt::operator-(const long long& num) const
{
    return *this - BigInt(num);
}

/*
    Integer - BigInt
    ----------------
*/

BigInt operator-(const long long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) - rhs;
}

/*
    BigInt * Integer
    ----------------
*/

BigInt BigInt::operator*(const long long& num) const
{
    return *this * BigInt(num);
}

/*
    Integer * BigInt
    ----------------
*/

BigInt operator*(const long long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) * rhs;
}

/*
    BigInt / Integer
    ----------------
*/

BigInt BigInt::operator/(const int& num) const
{
    return *this / BigInt(num);
}

BigInt BigInt::operator/(const unsigned int& num) const
{
    return *this / BigInt(num);
}

BigInt BigInt::operator/(const long& num) const
{
    return *this / BigInt(num);
}

BigInt BigInt::operator/(const unsigned long& num) const
{
    return *this / BigInt(num);
}

BigInt BigInt::operator/(const long long& num) const
{
    return *this / BigInt(num);
}

BigInt BigInt::operator/(const unsigned long long& num) const
{
    return *this / BigInt(num);
}

/*
    Integer / BigInt
    ----------------
*/

BigInt operator/(const int& lhs, const BigInt& rhs)
{
    return BigInt(lhs) / rhs;
}

BigInt operator/(const unsigned int& lhs, const BigInt& rhs)
{
    return BigInt(lhs) / rhs;
}

BigInt operator/(const long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) / rhs;
}

BigInt operator/(const unsigned long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) / rhs;
}

BigInt operator/(const long long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) / rhs;
}

BigInt operator/(const unsigned long long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) / rhs;
}

/*
    BigInt % Integer
    ----------------
*/

BigInt BigInt::operator%(const long long& num) const
{
    return *this % BigInt(num);
}

/*
    Integer % BigInt
    ----------------
*/

BigInt operator%(const long long& lhs, const BigInt& rhs)
{
    return BigInt(lhs) % rhs;
}

#endif  // BIG_INT_BINARY_ARITHMETIC_OPERATORS_HPP


/*
    ===========================================================================
    Arithmetic-assignment operators
    ===========================================================================
*/

#ifndef BIG_INT_ARITHMETIC_ASSIGNMENT_OPERATORS_HPP
#define BIG_INT_ARITHMETIC_ASSIGNMENT_OPERATORS_HPP

/*
    BigInt += BigInt
    ----------------
*/

BigInt& BigInt::operator+=(const BigInt& num)
{
    *this = *this + num;

    return *this;
}

/*
    BigInt -= BigInt
    ----------------
*/

BigInt& BigInt::operator-=(const BigInt& num)
{
    *this = *this - num;

    return *this;
}

/*
    BigInt *= BigInt
    ----------------
*/

BigInt& BigInt::operator*=(const BigInt& num)
{
    *this = *this * num;

    return *this;
}

/*
    BigInt /= BigInt
    ----------------
*/

BigInt& BigInt::operator/=(const BigInt& num)
{
    *this = *this / num;

    return *this;
}

/*
    BigInt %= BigInt
    ----------------
*/

BigInt& BigInt::operator%=(const BigInt& num)
{
    *this = *this % num;

    return *this;
}

/*
    BigInt += Integer
    -----------------
*/

BigInt& BigInt::operator+=(const int& num)
{
    *this = *this + BigInt(num);

    return *this;
}

BigInt& BigInt::operator+=(const unsigned int& num)
{
    *this = *this + BigInt(num);

    return *this;
}

BigInt& BigInt::operator+=(const long& num)
{
    *this = *this + BigInt(num);

    return *this;
}

BigInt& BigInt::operator+=(const unsigned long& num)
{
    *this = *this + BigInt(num);

    return *this;
}

BigInt& BigInt::operator+=(const long long& num)
{
    *this = *this + BigInt(num);

    return *this;
}

BigInt& BigInt::operator+=(const unsigned long long& num)
{
    *this = *this + BigInt(num);

    return *this;
}

/*
    BigInt -= Integer
    -----------------
*/

BigInt& BigInt::operator-=(const int& num)
{
    *this = *this - BigInt(num);

    return *this;
}

BigInt& BigInt::operator-=(const unsigned int& num)
{
    *this = *this - BigInt(num);

    return *this;
}

BigInt& BigInt::operator-=(const long& num)
{
    *this = *this - BigInt(num);

    return *this;
}

BigInt& BigInt::operator-=(const unsigned long& num)
{
    *this = *this - BigInt(num);

    return *this;
}

BigInt& BigInt::operator-=(const long long& num)
{
    *this = *this - BigInt(num);

    return *this;
}

BigInt& BigInt::operator-=(const unsigned long long& num)
{
    *this = *this - BigInt(num);

    return *this;
}

/*
    BigInt *= Integer
    -----------------
*/

BigInt& BigInt::operator*=(const long long& num)
{
    *this = *this * BigInt(num);

    return *this;
}

/*
    BigInt /= Integer
    -----------------
*/

BigInt& BigInt::operator/=(const long long& num)
{
    *this = *this / BigInt(num);

    return *this;
}

/*
    BigInt %= Integer
    -----------------
*/

BigInt& BigInt::operator%=(const long long& num)
{
    *this = *this % BigInt(num);

    return *this;
}

#endif  // BIG_INT_ARITHMETIC_ASSIGNMENT_OPERATORS_HPP


/*
    ===========================================================================
    Increment and decrement operators
    ===========================================================================
*/

#ifndef BIG_INT_INCREMENT_DECREMENT_OPERATORS_HPP
#define BIG_INT_INCREMENT_DECREMENT_OPERATORS_HPP

/*
    Pre-increment
    -------------
    ++BigInt
*/

BigInt& BigInt::operator++()
{
    *this += 1;

    return *this;
}

/*
    Pre-decrement
    -------------
    --BigInt
*/

BigInt& BigInt::operator--()
{
    *this -= 1;

    return *this;
}

/*
    Post-increment
    --------------
    BigInt++
*/

BigInt BigInt::operator++(int)
{
    BigInt temp = *this;
    *this += 1;

    return temp;
}

/*
    Post-decrement
    --------------
    BigInt--
*/

BigInt BigInt::operator--(int)
{
    BigInt temp = *this;
    *this -= 1;

    return temp;
}

#endif  // BIG_INT_INCREMENT_DECREMENT_OPERATORS_HPP

BigInt BigInt::pow(uint32_t exponent) const
{
    BigInt result;
    check_tommath_result(mp_expt_u32(&get_data(), exponent, &result.data));

    return result;
}

// Utility methods borrowed from Boost.Multiprecision

// Multiply t with o, updating t with the result. NOTE: because this mutates t, it should be avoided whenever possible;
// use the immutability-safe operator* unless absolutely necessary.
inline void eval_multiply(BigInt& t, const BigInt& o)
{
    check_tommath_result(mp_mul(&t.get_data(), const_cast< ::mp_int*>(&o.get_data()), &t.get_data()));
}

inline void eval_add(BigInt& t, const BigInt& o)
{
    check_tommath_result(mp_add(&t.get_data(), const_cast< ::mp_int*>(&o.get_data()), &t.get_data()));
}

inline int eval_get_sign(const BigInt& val)
{
    return mp_iszero(&val.get_data()) ? 0 : mp_isneg(&val.get_data()) ? -1 : 1;
}
