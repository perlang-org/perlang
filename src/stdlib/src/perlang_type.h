#include "perlang_string.h"

namespace perlang
{
    class PerlangType
    {
     private:
        std::shared_ptr<String> name_;

     public:
        PerlangType(const char *name);

        std::shared_ptr<String> get_name();
    };
};
