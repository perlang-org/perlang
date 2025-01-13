#pragma once

#include <vector>

#include "perlang_string.h"

namespace perlang::text
{
    class StringBuilder
    {
     private:
        // TODO: Make this be calculated dynamically based on average sizes of previous StringBuilders that has been
        // TODO: created. Implementing this should be fairly trivial by two extra static fields: num_instances and
        // TODO: total_size. The problem is that these should be atomic, so we need some locking primitives in place
        // TODO: to make the implementation be thread safe.
        const uint DEFAULT_BUFFER_SIZE = 1024;

        std::vector<char>* buffer_;
        uint buffer_capacity_;
        uint current_position_ = 0;
        uint length_ = 0;

     public:
        StringBuilder();
        ~StringBuilder();

        void append(const perlang::String& str);
        void append_line(const String& str);

        [[nodiscard]]
        uint length() const;

        std::unique_ptr<perlang::String> to_string();
    };
}