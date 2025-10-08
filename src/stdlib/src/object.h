#pragma once

#include <memory>

namespace perlang
{
    // Forward reference because of circular dependency, and cannot include the header file because it has a base class
    // dependency on types defined in this file.
    class String;

    class Object
    {
     public:
        virtual ~Object() = default;

        [[nodiscard]]
        std::unique_ptr<String> get_type() const;

        // Expected to be overridden by child classes, to provide an implementation more suitable for a particular type.
        [[nodiscard]]
        virtual std::shared_ptr<const String> to_string() const;

        static std::unique_ptr<Object> convert_from(int value);

        static std::shared_ptr<Object> convert_from(std::shared_ptr<String> value);
    };
}
