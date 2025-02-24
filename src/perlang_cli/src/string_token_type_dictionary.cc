#include "perlang_cli.h"
#include "string_token_type_dictionary.h"

StringTokenTypeDictionary::StringTokenTypeDictionary(MutableStringTokenTypeDictionary& source)
{
    for (const auto& item: source.keys()) {
        data_[item] = source.get(item);
    }
}

bool StringTokenTypeDictionary::contains_key(const char* key)
{
    // TODO: Replace with contains() once we are on C++20
    return data_.count(perlang::ASCIIString::from_copied_string(key)) == 1;
}

TokenType::TokenType StringTokenTypeDictionary::get(const char* key)
{
    return data_[perlang::ASCIIString::from_copied_string(key)];
}
