#pragma once

#include "ascii_string.h"
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
        std::unique_ptr<String> get_type() const override
        {
            return ASCIIString::from_static_string("perlang.Integer");
        }

        [[nodiscard]]
        std::shared_ptr<const String> to_string() const override;
    };
}
