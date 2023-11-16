/*
    BigInt
    ------
    Arbitrary-sized integer class for C++.

    Version: 0.5.0-dev
    Released on: 05 October 2020 23:15 IST
    Author: Syed Faheel Ahmad (faheel@live.in)
    Project on GitHub: https://github.com/faheel/BigInt
    License: MIT
*/

/*
    ===========================================================================
    BigInt
    ===========================================================================
    Definition for the BigInt class.
*/

#ifndef BIG_INT_HPP
#define BIG_INT_HPP

#include <iostream>

class BigInt {
    std::string value;
    char sign;

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
        BigInt(const std::string&);

        // Assignment operators:
        BigInt& operator=(const BigInt&);
        BigInt& operator=(const long long&);
        BigInt& operator=(const std::string&);

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
        BigInt operator+(const std::string&) const;
        BigInt operator-(const std::string&) const;
        BigInt operator*(const std::string&) const;
        BigInt operator/(const std::string&) const;
        BigInt operator%(const std::string&) const;

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
        BigInt& operator+=(const std::string&);
        BigInt& operator-=(const std::string&);
        BigInt& operator*=(const std::string&);
        BigInt& operator/=(const std::string&);
        BigInt& operator%=(const std::string&);

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
        bool operator<(const std::string&) const;
        bool operator>(const std::string&) const;
        bool operator<=(const std::string&) const;
        bool operator>=(const std::string&) const;
        bool operator==(const std::string&) const;
        bool operator!=(const std::string&) const;

        // I/O stream operators:
        friend std::istream& operator>>(std::istream&, BigInt&);
        friend std::ostream& operator<<(std::ostream&, const BigInt&);

        // Conversion functions:
        std::string to_string() const;
        int to_int() const;
        long to_long() const;
        long long to_long_long() const;

        // Random number generating functions:
        friend BigInt big_random(size_t);
};

//// The following operator overloads operate with primitives like long long on the left-hand side; they are not part of
//// the class. Their declarations must still exist in the header file, so that it can be found by the calling code.

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

#endif  // BIG_INT_HPP

