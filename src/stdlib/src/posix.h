#pragma once

#ifndef _WIN32

#include <cstdint>
#include <sys/types.h>
#include <unistd.h>

/// Provides support for calling standard POSIX functions.
///
/// The method names are deliberately using "POSIX-style", i.e. short, C-oriented names for methods like `getuid`,
/// `getpid` and so forth. This is to make it simple for people with experience from POSIX-based systems to find the
/// method they are looking for.
///
/// This class only contains POSIX-specific functions, i.e. functions available on POSIX-compliant systems like BSD,
/// GNU/Linux and macOS, but not available on Windows. For C functions available on all supported platforms, see the
/// list below.
///
/// Code which uses this class will only run on POSIX-compliant systems. If an attempt to use these methods is made
/// on Windows, a compile-time error is emitted.
///
/// ### List of methods defined in POSIX but available elsewhere:
///
/// * Libc.environ
/// * Libc.getcwd
/// * Libc.getenv
/// * Libc.getpid
///
/// The method descriptions are based on `man` pages in [the NetBSD source code](https://github.com/NetBSD/src).
/// The full license of these man pages can be found at https://gitlab.perlang.org/perlang/perlang/-/blob/master/NOTICE.md
///
/// There might be subtle differences between systems on some of these functions. For information on how these work
/// on e.g. GNU/Linux, use a command like `man 2 getgid` (replace `getgid` with the name of the function you are
/// interested in). You can also consult the Linux man pages project:
/// https://man7.org/linux/man-pages/dir_all_alphabetic.html/.
namespace perlang
{
    class Posix
    {
     public:
        /// \brief The getgid() function returns the real group ID of the calling process; getegid() returns the
        /// effective group ID of the calling process.
        ///
        /// The real group ID is specified at login time.
        ///
        /// The real group ID is the group of the user who invoked the program. As the effective group ID gives the
        /// process additional permissions during the execution of `set-group-ID` mode processes, getgid() is used to
        /// determine the real-group-id of the calling process.
        ///
        /// getgid() and getegid() conform to ISO/IEC 9945-1:1990 (`POSIX.1`).
        ///
        /// \returns The effective group ID of the calling process.
        /// \see getuid()
        // TODO: add when we support it: \see setgid()
        // TODO: add when we support it: \see setgroups()
        // TODO: add when we support it: \see setregid()
        static gid_t getegid()
        {
            return ::getegid();
        }

        /// \brief The getuid() function returns the real user ID of the calling process. The geteuid() function returns
        /// the effective user ID of the calling process.
        ///
        /// The real user ID is that of the user who has invoked the program. As the effective user ID gives the
        /// process additional permissions during execution of `set-user-ID` mode processes, getuid() is used to
        /// determine the real-user-id of the calling process.
        ///
        /// The geteuid() and getuid() functions conform to ISO/IEC 9945-1:1990 (`POSIX.1`).
        ///
        /// \returns The effective user ID of the calling process.
        /// \see getgid()
        // TODO: add when we support it: \see setreuid()
        static uid_t geteuid()
        {
            return ::geteuid();
        }

        /// \brief The getgid() function returns the real group ID of the calling process; getegid() returns the
        /// effective group ID of the calling process.
        ///
        /// The real group ID is specified at login time.
        ///
        /// The real group ID is the group of the user who invoked the program. As the effective group ID gives the
        /// process additional permissions during the execution of `set-group-ID` mode processes, getgid() is used to
        /// determine the real-group-id of the calling process.
        ///
        /// getgid() and getegid() conform to ISO/IEC 9945-1:1990 (`POSIX.1`).
        ///
        /// \returns The real group ID of the calling process.
        // TODO: add when we support it: \see setgid()
        // TODO: add when we support it: \see setgroups()
        // TODO: add when we support it: \see setregid()
        static gid_t getgid()
        {
            return ::getgid();
        }

        /// \brief Returns the process ID of the parent of the calling process.
        ///
        /// getppid() conforms to ISO/IEC 9945-1:1990 (`POSIX.1`).
        ///
        /// \returns The parent process ID.
        /// \see Libc::getpid()
        static pid_t getppid()
        {
            return ::getppid();
        }

        /// \brief The getuid() function returns the real user ID of the calling process. The geteuid() function returns
        /// the effective user ID of the calling process.
        ///
        /// The real user ID is that of the user who has invoked the program. As the effective user ID gives the
        /// process additional permissions during execution of `set-user-ID` mode processes, getuid() is used to
        /// determine the real-user-id of the calling process.
        ///
        /// The geteuid() and getuid() functions conform to ISO/IEC 9945-1:1990 (`POSIX.1`).
        ///
        /// \returns The real user ID of the calling process.
        /// \see getgid()
        // TODO: add when we support it: \see setreuid()
        static uid_t getuid()
        {
            return ::getuid();
        }
    };
}
#endif // !_WIN32
