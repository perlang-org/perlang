#include <stdexcept>
#include <cwctype>

#include "perlang_char.h"

std::unique_ptr<perlang::Char> perlang::Char::from(const char16_t literal)
{
    auto perlang_char = new perlang::Char(literal);
    return std::unique_ptr<perlang::Char>(perlang_char);
}

char16_t perlang::Char::to_upper(char16_t literal)
{
    return std::towupper(literal);
}

char16_t perlang::Char::to_lower(char16_t literal)
{
    return std::towlower(literal);
}

std::shared_ptr<const perlang::String> perlang::Char::to_string() const
{
    throw std::runtime_error("not yet implemented");
}
