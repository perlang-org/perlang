#pragma once

#include <tsl/ordered_map.h>

#include "common.h"
#include "perlang_cli.h"
#include "perlang_stdlib.h"

class MutableStringTokenTypeDictionary {
private:
    tsl::ordered_map<std::shared_ptr<perlang::String>, TokenType::TokenType, string_hasher, string_comparer> data_;

public:
    // These must be manually defined because CppSharp will get errors attempting to call them otherwise. It probably
    // works differently from C++, because the compiler is smart enough to know that no explicit constructor need to be
    // called when not defined.
    MutableStringTokenTypeDictionary();
    ~MutableStringTokenTypeDictionary();

    void add(const char *, TokenType::TokenType);
    bool contains_key(const char *);
    std::vector<std::shared_ptr<perlang::String>> keys();
    TokenType::TokenType get(const char *);
    TokenType::TokenType get(std::shared_ptr<perlang::String>);
};
