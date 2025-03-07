#include "mutable_string_hash_set.h"
#include "ascii_string.h"

namespace perlang::collections
{
    MutableStringHashSet::MutableStringHashSet() = default;

    void MutableStringHashSet::add(const char* value)
    {
        const std::shared_ptr<perlang::ASCIIString>& value_string = perlang::ASCIIString::from_copied_string(value);
        data_.insert(value_string);
    }

    void MutableStringHashSet::add(const std::shared_ptr<perlang::String>& value)
    {
        data_.insert(value);
    }

    bool MutableStringHashSet::contains(const char* value)
    {
        // TODO: Replace with contains() once we are on C++20
        return data_.count(perlang::ASCIIString::from_copied_string(value)) == 1;
    }

    std::vector<std::shared_ptr<perlang::String>> MutableStringHashSet::values()
    {
        std::vector<std::shared_ptr<perlang::String>> values;

        for (auto& key : data_) {
            values.push_back(key);
        }

        return values;
    }
}
