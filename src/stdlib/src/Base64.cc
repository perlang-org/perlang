#include "perlang_stdlib.h"

namespace perlang::stdlib
{
    [[maybe_unused]] std::shared_ptr<const ASCIIString> Base64::to_string()
    {
        // Poor-man's FQCN. :-)
        return ASCIIString::from_static_string("Perlang.Stdlib.Base64");
    }
}
