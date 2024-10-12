#pragma once

#include <memory>

#include "perlang_string.h"

namespace perlang::io
{
    class File
    {
    public:
        // Reads a file from the given path and returns its contents as a string. The file is presumed to be encoded in
        // UTF-8.
        static std::unique_ptr<const String> read_all_text(const String &path);
    };
}
