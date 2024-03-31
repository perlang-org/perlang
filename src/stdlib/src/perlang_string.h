#pragma once

namespace perlang
{
    // Abstract base class for all String types in Perlang
    class String
    {
     public:
        // Returns the backing byte array for this String. This method is generally to be avoided; it is safer to use
        // the String throughout the code and only call this when you really must.
        [[nodiscard]]
        virtual const char* bytes() const = 0;
    };
}