#pragma once

#include <tsl/ordered_set.h>

#include "text/string_hashing.h"

namespace perlang::collections
{
    // Used for returning data to C#
    struct StringArray {
        const char** items;
        unsigned long size;
    };

    class StringHashSet
    {
     private:
        tsl::ordered_set<std::shared_ptr<perlang::String>, string_hasher, string_comparer> data_;

     public:
        // These must exist since CppSharp attempts to call them
        explicit StringHashSet(const StringHashSet &);
        ~StringHashSet();

        explicit StringHashSet(MutableStringHashSet&);
        bool contains(const char*);
        StringHashSet concat(const StringHashSet&);
        std::vector<std::shared_ptr<perlang::String>> values();

        // Interop-oriented version of the above, callable from CppSharp. Note that the memory allocated by this method
        // must be freed by calling delete_values_wrapper_result() to avoid leaking memory.
        StringArray values_wrapper();

        void delete_values_wrapper_result(StringArray string_array);
    };
}
