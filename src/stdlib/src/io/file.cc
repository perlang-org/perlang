#include <cstdio>
#include <stdexcept>
#include <string>

#include "io/file.h"
#include "utf8_string.h"

namespace perlang::io
{
    [[nodiscard]]
    std::unique_ptr<String> File::read_all_text(const String& path)
    {
        FILE *file = fopen(path.bytes(), "r");

        if (file == nullptr) {
            // TODO: Should we throw an exception at this point?
            return nullptr;
        }

        // Determine the size of the file
        fseek(file, 0, SEEK_END);
        long length = ftell(file);

        if (length == -1) {
            fclose(file);
            return nullptr;
        }

        // Go back to the beginning and read the file into a newly allocated buffer
        fseek(file, 0, SEEK_SET);

        auto buffer = std::make_unique<char[]>(length + 1);

        if (buffer == nullptr) {
            fclose(file);
            throw std::runtime_error("Failed to allocate memory when attempting to read file " + std::string(path.bytes()));
        }

        size_t read = fread(buffer.get(), 1, length, file);
        fclose(file);

        // Ensure that we read the entire file; if not, return an error to the caller
        if (read != (size_t)length) {
            throw std::runtime_error("Expected to read " + std::to_string(length) + " bytes, but only read " + std::to_string(read) + " bytes");
        }

        // Create a Perlang string based on the newly read data. Releasing the buffer afterwards is crucial to avoid
        // double-free()ing the memory.
        buffer[length] = '\0';
        std::unique_ptr<String> ptr = UTF8String::from_owned_string(buffer.release(), length);

        return ptr;
    }
}
