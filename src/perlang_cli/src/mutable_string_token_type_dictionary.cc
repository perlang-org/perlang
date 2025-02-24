#include "perlang_cli.h"
#include "mutable_string_token_type_dictionary.h"

MutableStringTokenTypeDictionary::MutableStringTokenTypeDictionary() = default;
MutableStringTokenTypeDictionary::~MutableStringTokenTypeDictionary() = default;

void MutableStringTokenTypeDictionary::add(const char* key, TokenType::TokenType value)
{
    const std::shared_ptr<perlang::ASCIIString>& key_string = perlang::ASCIIString::from_copied_string(key);
    data_.insert_or_assign(key_string, value);
}

bool MutableStringTokenTypeDictionary::contains_key(const char* key)
{
    // TODO: Replace with contains() once we are on C++20
    return data_.count(perlang::ASCIIString::from_copied_string(key)) == 1;
}

std::vector<std::shared_ptr<perlang::String>> MutableStringTokenTypeDictionary::keys()
{
    std::vector<std::shared_ptr<perlang::String>> keys;

    for (auto& key: data_) {
        keys.push_back(key.first);
    }

    return keys;
}

TokenType::TokenType MutableStringTokenTypeDictionary::get(const char* key)
{
    return data_[perlang::ASCIIString::from_copied_string(key)];
}

TokenType::TokenType MutableStringTokenTypeDictionary::get(std::shared_ptr<perlang::String> key)
{
    return data_[key];
}
