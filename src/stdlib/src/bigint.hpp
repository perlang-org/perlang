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

/*
    ===========================================================================
    BigInt
    ===========================================================================
    Definition for the BigInt class.
*/

#ifndef BIG_INT_HPP
#define BIG_INT_HPP

#include "libtommath/tommath.h"

#include <string>
#include <cassert>

class BigInt {
    private:
        mp_int data;

    public:
        // Constructors:
        BigInt();
        BigInt(const BigInt&);
        BigInt(const int&);
        BigInt(const unsigned int&);
        BigInt(const long&);
        BigInt(const unsigned long&);
        BigInt(const long long&);
        BigInt(const unsigned long long& num);
        BigInt(const double& num);
        BigInt(const char* s);

        // Destructor
        ~BigInt();

        // Assignment operators:
        BigInt& operator=(const BigInt&);
        BigInt& operator=(const int&);
        BigInt& operator=(const unsigned int&);
        BigInt& operator=(const long&);
        BigInt& operator=(const long long&);
        BigInt& operator=(const unsigned long long&);
        BigInt& operator=(const char* s);

        // Unary arithmetic operators:
        BigInt operator+() const;   // unary +
        BigInt operator-() const;   // unary -

        // Binary arithmetic operators:
        BigInt operator+(const BigInt&) const;
        BigInt operator-(const BigInt&) const;
        BigInt operator*(const BigInt&) const;
        BigInt operator/(const BigInt&) const;
        BigInt operator%(const BigInt&) const;
        BigInt operator+(const long long&) const;
        BigInt operator-(const long long&) const;
        BigInt operator*(const long long&) const;
        BigInt operator/(const int&) const;
        BigInt operator/(const unsigned int&) const;
        BigInt operator/(const long&) const;
        BigInt operator/(const unsigned long&) const;
        BigInt operator/(const long long&) const;
        BigInt operator/(const unsigned long long&) const;
        BigInt operator%(const long long&) const;
        BigInt operator<<(const int) const;
        BigInt operator>>(const int) const;

        // Arithmetic-assignment operators:
        BigInt& operator+=(const BigInt&);
        BigInt& operator-=(const BigInt&);
        BigInt& operator*=(const BigInt&);
        BigInt& operator/=(const BigInt&);
        BigInt& operator%=(const BigInt&);
        BigInt& operator+=(const int&);
        BigInt& operator+=(const unsigned int&);
        BigInt& operator+=(const long&);
        BigInt& operator+=(const unsigned long&);
        BigInt& operator+=(const long long&);
        BigInt& operator+=(const unsigned long long&);
        BigInt& operator-=(const int&);
        BigInt& operator-=(const unsigned int&);
        BigInt& operator-=(const long&);
        BigInt& operator-=(const unsigned long&);
        BigInt& operator-=(const long long&);
        BigInt& operator-=(const unsigned long long&);
        BigInt& operator*=(const long long&);
        BigInt& operator/=(const long long&);
        BigInt& operator%=(const long long&);

        // Increment and decrement operators:
        BigInt& operator++();       // pre-increment
        BigInt& operator--();       // pre-decrement
        BigInt operator++(int);     // post-increment
        BigInt operator--(int);     // post-decrement

        // Relational operators:
        bool operator<(const BigInt&) const;
        bool operator>(const BigInt&) const;
        bool operator<=(const BigInt&) const;
        bool operator>=(const BigInt&) const;
        bool operator==(const BigInt&) const;
        bool operator!=(const BigInt&) const;
        bool operator<(const long long&) const;
        bool operator>(const int&) const;
        bool operator>(const unsigned int&) const;
        bool operator>(const long&) const;
        bool operator>(const unsigned long&) const;
        bool operator>(const long long&) const;
        bool operator>(const unsigned long long&) const;
        bool operator<=(const int&) const;
        bool operator<=(const unsigned int&) const;
        bool operator<=(const long&) const;
        bool operator<=(const unsigned long&) const;
        bool operator<=(const long long&) const;
        bool operator<=(const unsigned long long&) const;
        bool operator>=(const int&) const;
        bool operator>=(const unsigned int&) const;
        bool operator>=(const long&) const;
        bool operator>=(const unsigned long&) const;
        bool operator>=(const long long&) const;
        bool operator>=(const unsigned long long&) const;

        bool operator==(const int&) const;
        bool operator==(const unsigned int&) const;
        bool operator==(const long&) const;
        bool operator==(const unsigned long&) const;
        bool operator==(const long long&) const;
        bool operator==(const unsigned long long&) const;
        bool operator==(const double&) const;

        bool operator!=(const int&) const;
        bool operator!=(const unsigned int&) const;
        bool operator!=(const long&) const;
        bool operator!=(const unsigned long&) const;
        bool operator!=(const long long&) const;
        bool operator!=(const unsigned long long&) const;
        bool operator!=(const double&) const;

        BigInt pow(uint32_t exponent) const;

        // Conversion functions:
        std::string to_string() const;

    ::mp_int& get_data()
        {
            assert(data.dp != nullptr);
            return data;
        }

        const ::mp_int& get_data() const
        {
            assert(data.dp != nullptr);
            return data;
        }
};

// The following operator overloads operate with primitives like long long on the left-hand side; they are not part of
// the class. Their declarations must still exist in the header file, so that it can be found by the calling code.

/*
    Integer / BigInt
    ----------------
*/

bool operator==(const int& lhs, const BigInt& rhs);
bool operator==(const unsigned int& lhs, const BigInt& rhs);
bool operator==(const long& lhs, const BigInt& rhs);
bool operator==(const unsigned long& lhs, const BigInt& rhs);
bool operator==(const long long& lhs, const BigInt& rhs);
bool operator==(const unsigned long long& lhs, const BigInt& rhs);
bool operator==(const double& lhs, const BigInt& rhs);

bool operator!=(const int& lhs, const BigInt& rhs);
bool operator!=(const unsigned int& lhs, const BigInt& rhs);
bool operator!=(const long& lhs, const BigInt& rhs);
bool operator!=(const unsigned long& lhs, const BigInt& rhs);
bool operator!=(const long long& lhs, const BigInt& rhs);
bool operator!=(const unsigned long long& lhs, const BigInt& rhs);
bool operator!=(const double& lhs, const BigInt& rhs);
bool operator<(const long long& lhs, const BigInt &rhs);
bool operator>(const int& lhs, const BigInt& rhs);
bool operator>(const unsigned int& lhs, const BigInt& rhs);
bool operator>(const long& lhs, const BigInt& rhs);
bool operator>(const unsigned long& lhs, const BigInt& rhs);
bool operator>(const long long& lhs, const BigInt& rhs);
bool operator>(const unsigned long long& lhs, const BigInt& rhs);
bool operator<=(const int& lhs, const BigInt& rhs);
bool operator<=(const unsigned int& lhs, const BigInt& rhs);
bool operator<=(const long& lhs, const BigInt& rhs);
bool operator<=(const unsigned long& lhs, const BigInt& rhs);
bool operator<=(const long long& lhs, const BigInt& rhs);
bool operator<=(const unsigned long long& lhs, const BigInt& rhs);
bool operator>=(const int& lhs, const BigInt& rhs);
bool operator>=(const unsigned int& lhs, const BigInt& rhs);
bool operator>=(const long& lhs, const BigInt& rhs);
bool operator>=(const unsigned long& lhs, const BigInt& rhs);
bool operator>=(const long long& lhs, const BigInt& rhs);
bool operator>=(const unsigned long long& lhs, const BigInt& rhs);
BigInt operator+(const long long& lhs, const BigInt& rhs);
BigInt operator-(const long long& lhs, const BigInt& rhs);
BigInt operator*(const long long& lhs, const BigInt& rhs);

BigInt operator/(const int& lhs, const BigInt& rhs);
BigInt operator/(const unsigned int& lhs, const BigInt& rhs);
BigInt operator/(const long& lhs, const BigInt& rhs);
BigInt operator/(const unsigned long& lhs, const BigInt& rhs);
BigInt operator/(const long long& lhs, const BigInt& rhs);
BigInt operator/(const unsigned long long& lhs, const BigInt& rhs);
BigInt operator%(const long long& lhs, const BigInt& rhs);

void eval_multiply(BigInt& t, const BigInt& o);
void eval_add(BigInt& t, const BigInt& o);
int eval_get_sign(const BigInt& val);

#endif  // BIG_INT_HPP
