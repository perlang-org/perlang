#pragma once

#include "ascii_string.h"
#include "object.h"

namespace perlang
{
    class Char : public Object
    {
     public:
        [[nodiscard]]
        static std::unique_ptr<Char> from(char16_t literal);

     private:
        explicit Char(const char16_t value)
            : value_(value)
        {
        }

     public:
        [[nodiscard]]
        std::unique_ptr<String> get_type() const override
        {
            return ASCIIString::from_static_string("perlang.Char");
        }

        [[nodiscard]]
        char16_t value() const
        {
            return value_;
        }

        [[nodiscard]]
        std::shared_ptr<const String> to_string() const override;

     private:
        const char16_t value_;
    };
}
