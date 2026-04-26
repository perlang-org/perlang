#pragma once

#include <cxxabi.h>
#include <execinfo.h>
#include <memory>
#include <stdexcept>
#include <string>
#include <vector>

#include "perlang_string.h"

namespace perlang
{
    /// <summary>
    /// Base class for all Perlang errors. Captures a stack trace at the point of construction, making it
    /// possible to display a meaningful backtrace when an error is handled.
    /// </summary>
    class Error
    {
     public:
        explicit Error(std::shared_ptr<const String> message)
        {
            message_ = std::move(message);

            void* raw_frames[64];
            int count = backtrace(raw_frames, 64);
            stack_frames_.resize(static_cast<size_t>(count));

            for (int i = 0; i < count; ++i) {
                stack_frames_[static_cast<size_t>(i)] = raw_frames[i];
            }
        }

        virtual ~Error() = default;

        [[nodiscard]]
        std::shared_ptr<const String> message() const
        {
            return message_;
        }

        // TODO: Could return something more idiomatic than std::string here, like ASCIIString built using
        // perlang::text::StringBuilder
        [[nodiscard]]
        std::string stack_trace() const
        {
            if (stack_frames_.empty()) {
                return "  (empty stack trace)\n";
            }

            char** symbols = backtrace_symbols(stack_frames_.data(), static_cast<int>(stack_frames_.size()));
            std::string result;

            for (size_t i = 0; i < stack_frames_.size(); ++i) {
                std::string sym(symbols[i]);

                // Each symbol has the format: "binary(mangled_name+offset) [address]"
                // Extract and demangle the symbol name for readability.
                size_t start = sym.find('(');
                size_t plus  = sym.find('+', start);

                if (start != std::string::npos && plus != std::string::npos) {
                    std::string mangled = sym.substr(start + 1, plus - start - 1);

                    result += "  at ";

                    // Symbols with no name (just an offset) or known libc bootstrap symbols are shown in full (e.g.
                    // /lib/x86_64-linux-gnu/libc.so.6(+0x29ca8) [0x7f2926034ca8]) so the library path and address are
                    // visible.
                    static const std::vector<std::string> known_externals = {
                        "_start", "__libc_start_main", "__libc_start_call_main"
                    };

                    bool is_external = mangled.empty() ||
                        std::find(known_externals.begin(), known_externals.end(), mangled) != known_externals.end();

                    if (is_external) {
                        result += sym;
                    }
                    else {
                        int status;
                        char* demangled = abi::__cxa_demangle(mangled.c_str(), nullptr, nullptr, &status);

                        if (status == 0 && demangled != nullptr) {
                            result += demangled;
                            free(demangled);
                        }
                        else {
                            // Demangling failed. Use the mangled symbol instead as a fallback.
                            result += mangled;
                        }
                    }
                }
                else {
                    result += "  at ";
                    result += sym;
                }

                result += "\n";
            }

            free(symbols);
            return result;
        }

     private:
        std::shared_ptr<const String> message_;
        std::vector<void*> stack_frames_;
    };
}
