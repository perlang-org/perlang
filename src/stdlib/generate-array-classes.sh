#!/usr/bin/env bash

#
# Rationale: The IntArray, LongArray etc. classes are very similar. Because we don't want to use C++ templates, we
# generate these classes from a shell script to keep them in sync.
#
# Note that the StringArray class is not generated from this because it's not exactly identical to the others.
#

create_file() {
    local _prefix=$1
    local _class_name=$2
    local _cpp_type=$3
    local _type_description=$4
    local _type_long_description=$5

    local _header_file=${_prefix}_array.h
    local _implementation_file=${_prefix}_array.cc

    cat <<EOF > src/${_header_file}
#include <initializer_list>

#include "bigint.h"

#pragma once

namespace perlang
{
    // A class for representing mutable, fixed-size arrays of ${_type_long_description}.
    class ${_class_name}
    {
     public:
        // Creates a new ${_class_name} from a copied array of ${_type_description}.
        explicit ${_class_name}(std::initializer_list<${_cpp_type}> arr);

        // Creates a new ${_class_name} of the given size.
        explicit ${_class_name}(size_t length);

     private:
        ${_class_name}(${_cpp_type}* arr, size_t length, bool owned);

     public:
        ~${_class_name}();

        ${_cpp_type} operator[](size_t index) const;

        // C++ doesn't support operator overloading for assignment. We workaround this by just providing a set method
        // instead.
        void set(size_t index, ${_cpp_type} value);

     private:
        ${_cpp_type}* arr_;
        size_t length_;
        bool owned_;
    };
}
EOF

    cat <<EOF > src/${_implementation_file}
#include <memory.h>
#include <stdexcept>
#include <string>

#include "exceptions/null_pointer_exception.h"
#include "${_header_file}"

namespace perlang
{
    ${_class_name}::${_class_name}(std::initializer_list<${_cpp_type}> arr)
    {
        // Create a new buffer and copy the array into it.
        size_t length = arr.size();
        auto new_arr = (${_cpp_type}*)malloc(length * sizeof(${_cpp_type}));
        memcpy(new_arr, data(arr), length * sizeof(${_cpp_type}));

        arr_ = new_arr;
        length_ = length;
        owned_ = true;
    }

    ${_class_name}::${_class_name}(size_t length)
    {
        arr_ = (${_cpp_type}*)calloc(length, sizeof(${_cpp_type}));
        length_ = length;
        owned_ = true;
    }

    ${_class_name}::${_class_name}(${_cpp_type}* arr, size_t length, bool owned)
    {
        arr_ = arr;
        length_ = length;
        owned_ = owned;
    }

    ${_class_name}::~${_class_name}()
    {
        if (owned_) {
            free((void*)arr_);
        }
    }

    ${_cpp_type} ${_class_name}::operator[](size_t index) const
    {
        if (this == nullptr) {
            throw NullPointerException();
        }

        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        return arr_[index];
    }

    void ${_class_name}::set(size_t index, ${_cpp_type} value)
    {
        if (index >= length_) {
            throw std::out_of_range("index out of range (" + std::to_string(index) + " > " + std::to_string(length_ - 1) + ")");
        }

        arr_[index] = value;
    }
}
EOF
}

create_file int IntArray int32_t "integers" "32-bit signed integers"
create_file long LongArray int64_t "long integers" "64-bit signed integers"
create_file uint UIntArray uint32_t "integers" "32-bit unsigned integers"
create_file ulong ULongArray uint64_t "integers" "64-bit unsigned integers"
create_file float FloatArray float "floats" "single-precision (32-bit) floating point values"
create_file double DoubleArray double "doubles" "double-precision (64-bit) floating point values"
create_file bool BoolArray bool "booleans" "boolean values"
create_file char CharArray char16_t "characters" "UTF-16LE characters"

# TODO: Doesn't match the generated content 100% right now, because of non-trivial initialization (can't memcpy from
# initializer list)
#create_file bigint BigIntArray BigInt "bit integers" "big (larger than 64-bit) integers"
