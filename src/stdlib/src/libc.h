#pragma once

#include <memory> // std::shared_ptr
#include <unistd.h>

#include "perlang_string.h"
#include "utf8_string.h"

namespace perlang
{
    /// \brief Provides support for calling standard C library functions.
    ///
    /// This class supplements the `Posix` class by implementing support for functions available on all
    /// supported platforms.
    ///
    /// The method descriptions are based on [the .NET source
    /// code](https://github.com/dotnet/dotnet-api-docs/blob/main/xml/System/Environment.xml), licensed
    /// under the MIT license. Copyright (c) .NET Foundation and Contributors. Some method descriptions are also based
    /// on `man` pages in [the NetBSD source code](https://github.com/NetBSD/src). The full license of these man pages
    /// can be found at https://gitlab.perlang.org/perlang/perlang/-/blob/master/NOTICE.md.
    ///
    /// Portions may also be inspired by Donald Lewine's great book "POSIX Programmer's Guide" (O'Reilly 1991).
    class Libc
    {
     public:
        /// \brief Gets the fully qualified path of the current working directory.
        ///
        /// By definition, if this process starts in the root directory of a local or network drive, the value of this
        /// property is the drive name followed by a trailing slash (for example, `/` or `C:\\`). If this process starts in a
        /// subdirectory, the value of this property is the drive and subdirectory path, without a trailing slash (for
        /// example, `/usr/bin` or `C:\mySubDirectory`).
        ///
        /// \returns The current working directory.
        static std::shared_ptr<perlang::String> getcwd()
        {
            // glibc getcwd() allocates a large-enough buffer for us if we pass NULL as the buf parameter
            char *cwd = ::getcwd(nullptr, 0);
            auto result = perlang::UTF8String::from_copied_string(cwd);
            free(cwd);

            return result;
        }

        /// \brief Retrieves the value of an environment variable from the current process.
        ///
        /// The getenv() method retrieves an environment variable from the environment block of the current process.
        ///
        /// Environment variable names are case-sensitive on Linux and macOS and case-insensitive on Windows.
        ///
        /// ### On Linux and macOS systems
        ///
        /// On Linux and macOS, the environment block of the current process includes the following environment
        /// variables:
        ///
        /// - All environment variables that are provided to it by the parent process that created it. For applications
        ///   launched from a shell, this includes all environment variables defined in the shell.
        ///
        /// ### On Windows systems
        ///
        /// On Windows systems, the environment block of the current process includes:
        ///
        /// - All environment variables that are provided to it by the parent process that created it. For example, an
        ///   application launched from a console window inherits all of the console window's environment variables.
        ///
        /// - If there is no parent process, per-machine and per-user environment variables are used instead. For
        ///   example, a new console window has all per-machine and per-user environment variables defined at the
        ///   time it was launched.
        ///
        /// ### Standard environment variables
        ///
        /// In addition to user-defined environment variables, ISO/IEC 9945-1:1990 (`POSIX.1`) defines a set of
        /// standard environment variables. See environ() for more details on these
        ///
        /// \param name The name of the environment variable.
        /// \returns The value of the environment variable specified by \p name, or \c null if the environment variable
        ///          is not found.
        // TODO: ArgumentNullException \p name is \c null.
        static std::shared_ptr<perlang::String> getenv(const std::shared_ptr<perlang::String>& name)
        {
            // from_static_string() could probably work, but I don't think the return value of getenv() is _guaranteed_
            // to be statically allocated, so making a copy is safer.
            return perlang::UTF8String::from_static_string(::getenv(name->bytes()));
        }

        /// \brief Returns the process ID of the calling process.
        ///
        /// The ID is guaranteed to be unique and it might be tempting to use it for constructing temporary file names.
        /// However, this is _NOT_ recommended since it is not secure. In C, `mkstemp` can be used for this purpose.
        /// `mkstemp` is not yet available to use from Perlang, though.
        ///
        /// getpid() conforms to ISO/IEC 9945-1:1990 (`POSIX.1`).
        ///
        /// \returns The process ID of the calling process.
        /// \see Posix::getppid()
        static pid_t getpid()
        {
            return ::getpid();
        }
    };
}
