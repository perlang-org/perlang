#pragma once

#include "error.h"
#include "perlang_string.h"
#include "utf8_string.h"

namespace perlang
{
    class ArgumentError : public Error
    {
     public:
        explicit ArgumentError(std::shared_ptr<const String> message)
            : Error(message)
        {
        }

        explicit ArgumentError(const std::string& message)
            : Error(UTF8String::from_copied_string(message.c_str(), message.length()))
        {
        }
    };
}
