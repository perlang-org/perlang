#pragma once

#include <memory>
#include <stdexcept>

#include "perlang_string.h"

namespace perlang
{
    class IllegalStateException : public std::runtime_error
    {
     public:
        IllegalStateException(std::shared_ptr<String> message) :
            std::runtime_error(std::string(message.get()->bytes()))
        {
        }
    };
}
