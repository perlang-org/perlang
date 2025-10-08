#pragma once

#include "object.h"

namespace perlang
{
    class Integer : public Object
    {
     private:
        int value_;

     public:
        explicit Integer(const int value);

        [[nodiscard]]
        std::shared_ptr<const String> to_string() const override;
    };
}
