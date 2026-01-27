#!/usr/bin/env bash

#
# Rationale: The IntRange, LongRange etc. classes are very similar. Because we don't want to use C++ templates, we
# generate these classes from a shell script to keep them in sync.
#

create_file() {
    local _prefix=$1
    local _class_name=$2
    local _cpp_type=$3

    local _header_file=${_prefix}_range.h

    cat <<EOF > src/${_header_file}
#include <cstdint>

#pragma once

namespace perlang
{
    class ${_class_name}
    {
     private:
        ${_cpp_type} begin_;
        ${_cpp_type} end_;

     public:
        static ${_class_name} from(${_cpp_type} begin, ${_cpp_type} end)
        {
            return ${_class_name}(begin, end);
        }

        ${_class_name}(${_cpp_type} begin, ${_cpp_type} end)
        {
            begin_ = begin;
            end_ = end;
        }

        [[nodiscard]]
        bool contains(${_cpp_type} i) const
        {
            return i >= begin_ &&
                   i <= end_;
        }
    };
}
EOF
}

create_file int IntRange int32_t
create_file long LongRange int64_t
create_file uint UIntRange uint32_t
create_file ulong ULongRange uint64_t
create_file bigint BigIntRange BigInt
create_file char CharRange char16_t
