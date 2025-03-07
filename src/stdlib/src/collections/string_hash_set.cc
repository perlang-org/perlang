#include "perlang_stdlib.h"
#include "string_hash_set.h"

namespace perlang::collections
{
    StringHashSet::StringHashSet(const StringHashSet&) = default;
    StringHashSet::~StringHashSet() = default;

    StringHashSet::StringHashSet(MutableStringHashSet& source)
    {
        for (const auto& item : source.values()) {
            data_.insert(item);
        }
    }

    bool StringHashSet::contains(const char* value)
    {
        // TODO: Replace with contains() once we are on C++20
        return data_.count(perlang::ASCIIString::from_copied_string(value)) == 1;
    }

    StringHashSet StringHashSet::concat(const StringHashSet& other)
    {
        MutableStringHashSet mutableResult;

        for (const auto& item : data_) {
            mutableResult.add(item);
        }

        for (const auto& item : other.data_) {
            mutableResult.add(item);
        }

        return StringHashSet(mutableResult);
    }

    std::vector<std::shared_ptr<perlang::String>> StringHashSet::values()
    {
        std::vector<std::shared_ptr<perlang::String>> values;

        for (auto& key : data_) {
            values.push_back(key);
        }

        return values;
    }

    StringArray StringHashSet::values_wrapper()
    {
        StringArray result {
            .items = new const char*[data_.size()],
            .size = data_.size()
        };

        int index = 0;

        for (const auto& key : data_) {
            // Note: this approach means that the value returned from values_wrapper() can only safely ne used during
            // the lifetime of the containing StringHashSet; after it has been destroyed, the backing buffer may no
            // longer exist.
            result.items[index++] = key.get()->bytes();
        }

        return result;
    }
}
