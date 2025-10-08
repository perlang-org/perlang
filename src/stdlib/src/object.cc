#include "ascii_string.h"
#include "integer.h"
#include "object.h"

namespace perlang
{
    std::unique_ptr<String> Object::get_type() const
    {
        return ASCIIString::from_static_string("perlang.Object");
    }

    std::shared_ptr<const String> Object::to_string() const
    {
        return get_type();
    }

    std::unique_ptr<Object> Object::convert_from(int value)
    {
        auto boxed_value = new Integer(value);
        return std::unique_ptr<Integer>(boxed_value);
    }

    std::shared_ptr<Object> Object::convert_from(std::shared_ptr<String> value)
    {
        return value;
    }
}
