#pragma warning disable SA1300
#pragma warning disable SA1629
#pragma warning disable S3218
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Perlang.Attributes;

namespace Perlang.Stdlib
{
    /// <summary>
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
    /// * <see cref="Libc.environ"/>
    /// * <see cref="Libc.getcwd"/>
    /// * <see cref="Libc.getenv"/>
    /// * <see cref="Libc.getpid"/>
    /// </summary>
    /// <remarks>
    /// The XML method descriptions are based on `man` pages in [the NetBSD source code](https://github.com/NetBSD/src).
    /// The full license of these man pages can be found at https://github.com/perlang-org/perlang/blob/master/NOTICE.md
    ///
    /// There might be subtle differences between systems on some of these functions. For information on how these work
    /// on e.g. GNU/Linux, use a command like `man 2 getgid` (replace `getgid` with the name of the function you are
    /// interested in). You can also consult the Linux man pages project:
    /// https://man7.org/linux/man-pages/dir_all_alphabetic.html/.
    /// </remarks>
    [GlobalClass(PlatformID.Unix, PlatformID.MacOSX)]
    public static class Posix
    {
        // Internal class which contains the P/Invoke definitions, to avoid exposing them directly to our callers.
        [EditorBrowsable(EditorBrowsableState.Never)]
        private static class Internal
        {
            [DllImport("libc")]
            public static extern int getegid();

            [DllImport("libc")]
            public static extern int geteuid();

            [DllImport("libc")]
            public static extern int getgid();

            [DllImport("libc")]
            public static extern int getppid();

            [DllImport("libc")]
            public static extern int getuid();
        }

        /// <summary>
        /// The <see cref="getgid">`getgid()`</see> function returns the real group ID of the calling process, <see
        /// cref="getegid">getegid()</see> returns the effective group ID of the calling process.
        ///
        /// The real group ID is specified at login time.
        ///
        /// The real group ID is the group of the user who invoked the program.  As the effective group ID gives the
        /// process additional permissions during the execution of `set-group-ID` mode processes, <see
        /// cref="getgid">getgid()</see> is used to determine the real-group-id of the calling process.
        ///
        /// <see cref="getgid">getgid()</see> and <see cref="getegid">getegid()</see> conform to ISO/IEC 9945-1:1990
        /// (`POSIX.1`).
        /// </summary>
        /// <seealso cref="getuid"/>
        /// <returns>The effective group ID of the calling process.</returns>
        // TODO: add when we support it: <seealso cref="setgid"/>
        // TODO: add when we support it: <seealso cref="setgroups"/>
        // TODO: add when we support it: <seealso cref="setregid"/>
        public static int getegid()
        {
            return Internal.getegid();
        }

        /// <summary>
        /// The <see cref="getuid">getuid()</see> function returns the real user ID of the calling process. The <see
        /// cref="geteuid">geteuid()</see> function returns the effective user ID of the calling process.
        ///
        /// The real user ID is that of the user who has invoked the program.  As the effective user ID gives the
        /// process additional permissions during execution of `set-user-ID` mode processes, <see
        /// cref="getuid">getuid()</see> is used to determine the real-user-id of the calling process.
        ///
        /// The <see cref="geteuid">geteuid()</see> and <see cref="getuid">getuid()</see> functions conform to ISO/IEC
        /// 9945-1:1990 (`POSIX.1`).
        /// </summary>
        /// <returns>The effective user ID of the calling process.</returns>
        /// <seealso cref="getgid"/>
        // TODO: add when we support it: <seealso cref="setreuid"/>
        public static int geteuid()
        {
            return Internal.geteuid();
        }

        /// <summary>
        /// The <see cref="getgid">`getgid()`</see> function returns the real group ID of the calling process, <see
        /// cref="getegid">getegid()</see> returns the effective group ID of the calling process.
        ///
        /// The real group ID is specified at login time.
        ///
        /// The real group ID is the group of the user who invoked the program.  As the effective group ID gives the
        /// process additional permissions during the execution of `set-group-ID` mode processes, <see
        /// cref="getgid">getgid()</see> is used to determine the real-group-id of the calling process.
        ///
        /// <see cref="getgid">getgid()</see> and <see cref="getegid">getegid()</see> conform to ISO/IEC 9945-1:1990
        /// (`POSIX.1`).
        /// </summary>
        /// <returns>The real group ID of the calling process.</returns>
        // TODO: add when we support it: <seealso cref="setgid"/>
        // TODO: add when we support it: <seealso cref="setgroups"/>
        // TODO: add when we support it: <seealso cref="setregid"/>
        public static int getgid()
        {
            return Internal.getgid();
        }

        /// <summary>
        /// Returns the process ID of the parent of the calling process.
        ///
        /// <see cref="getppid">getppid()</see> conform to ISO/IEC 9945-1:1990 (`POSIX.1`).
        /// </summary>
        /// <returns>The parent process ID.</returns>
        /// <seealso cref="Libc.getpid"/>
        public static int getppid()
        {
            return Internal.getppid();
        }

        /// <summary>
        /// The <see cref="getuid">getuid()</see> function returns the real user ID of the calling process. The <see
        /// cref="geteuid">geteuid()</see> function returns the effective user ID of the calling process.
        ///
        /// The real user ID is that of the user who has invoked the program.  As the effective user ID gives the
        /// process additional permissions during execution of `set-user-ID` mode processes, <see
        /// cref="getuid">getuid()</see> is used to determine the real-user-id of the calling process.
        ///
        /// The <see cref="geteuid">geteuid()</see> and <see cref="getuid">getuid()</see> functions conform to ISO/IEC
        /// 9945-1:1990 (`POSIX.1`).
        /// </summary>
        /// <returns>The real user ID of the calling process.</returns>
        /// <seealso cref="getgid"/>
        // TODO: add when we support it: <seealso cref="setreuid"/>
        public static int getuid()
        {
            return Internal.getuid();
        }
    }
}
