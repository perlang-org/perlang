#include <string>

#include "ascii_string.h"
#include "integer.h"

perlang::Integer::Integer(const int value)
{
    value_ = value;
}

std::shared_ptr<const perlang::String> perlang::Integer::to_string() const
{
    return ASCIIString::from_copied_string(std::to_string(value_).c_str());
}
