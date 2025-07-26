#include <memory>

#include "perlang_type.h"
#include "perlang_string.h"
#include "utf8_string.h"

perlang::PerlangType::PerlangType(const char* name)
{
    name_ = perlang::UTF8String::from_copied_string(name);
}

std::shared_ptr<perlang::String> perlang::PerlangType::get_name()
{
    return name_;
}
