#pragma once

#include <tsl/ordered_set.h>

#include "text/string_hashing.h"

namespace perlang::collections
{
    class MutableStringHashSet
    {
     private:
        tsl::ordered_set<std::shared_ptr<perlang::String>, string_hasher, string_comparer> data_;

     public:
        // These must be manually defined because CppSharp will get errors attempting to call them otherwise. It probably
        // works differently from C++, because the compiler is smart enough to know that no explicit constructor need to be
        // called when not defined.
        MutableStringHashSet();

        void add(const char*);
        void add(const std::shared_ptr<perlang::String>&);
        bool contains(const char*);
        std::vector<std::shared_ptr<perlang::String>> values();
    };
}
