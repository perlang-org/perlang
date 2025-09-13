#include <stdexcept>

namespace perlang
{
    class NullPointerException : public std::runtime_error
    {
        public:
            NullPointerException() :
                std::runtime_error("NullPointerException: Attempting to dereference null pointer") {
            }
    };
}
