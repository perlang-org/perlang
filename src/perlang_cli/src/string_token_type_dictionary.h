#include <tsl/ordered_map.h>

#include "common.h"
#include "mutable_string_token_type_dictionary.h"
#include "perlang_cli.h"
#include "perlang_stdlib.h"

class StringTokenTypeDictionary {
private:
    tsl::ordered_map<std::shared_ptr<perlang::String>, TokenType::TokenType, string_hasher, string_comparer> data_;

public:
    StringTokenTypeDictionary(MutableStringTokenTypeDictionary&);
    bool contains_key(const char *);
    TokenType::TokenType get(const char *);
};
