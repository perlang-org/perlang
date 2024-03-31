#include "stdlib.hpp"

namespace perlang::stdlib
{
    ASCIIString Base64::to_string()
    {
        // Poor-man's FQCN. :-)
        return ASCIIString::from_static_string("Perlang.Stdlib.Base64");
    }
}
