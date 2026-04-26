#pragma once

#include "error.h"

namespace perlang
{
    class ArgumentError : public Error
    {
     public:
        explicit ArgumentError(std::shared_ptr<const String> message)
            : Error(message)
        {
        }
    };
}
