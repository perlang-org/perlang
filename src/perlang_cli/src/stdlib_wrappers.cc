// Wrappers on top of the Perlang standard library, to make it usable from C#
//
// These wrappers must take care of a few things:
//
// - Make C++ methods accessible from C# (by wrapping them in extern "C" functions)
// - Handle conversion from Perlang/C++ shared_ptr types to types that can be consumed from the C# side

#include <stdexcept>

#include "perlang_stdlib.h"

using namespace perlang;

extern "C"
{
    const char* File_read_all_text(const char* path)
    {
        std::unique_ptr<String> file_contents = perlang::io::File::read_all_text(*UTF8String::from_copied_string(path));

        // Take ownership of the backing byte array at this point, and let the caller be responsible for freeing it once
        // they are done with it.
        std::unique_ptr<const char[]> result = file_contents->release_bytes();
        return result.release();
    }

    void File_read_all_text_free(const char* s)
    {
        delete[] s;
    }

    perlang::text::StringBuilder* StringBuilder_new()
    {
        return new perlang::text::StringBuilder();
    }

    void StringBuilder_delete(perlang::text::StringBuilder* sb)
    {
        delete sb;
    }

    void StringBuilder_append(perlang::text::StringBuilder* sb, const char* s)
    {
        if (s == nullptr) {
            return;
        }

        sb->append(*UTF8String::from_copied_string(s));
    }

    void StringBuilder_append_line(perlang::text::StringBuilder* sb, const char* s)
    {
        if (s == nullptr) {
            sb->append_line(ASCIIString::empty);
            return;
        }

        sb->append_line(UTF8String::from_copied_string(s));
    }

    uint StringBuilder_length(perlang::text::StringBuilder* sb)
    {
        return sb->length();
    }

    const char* StringBuilder_to_string(perlang::text::StringBuilder* sb)
    {
        std::unique_ptr<String> result = sb->to_string();
        return result->release_bytes().release();
    }

    void StringBuilder_delete_to_string_result(const char* s)
    {
        delete[] s;
    }
}
