#pragma warning disable SA1300
#pragma warning disable S3218
using System;
using System.Runtime.InteropServices;
using Perlang.Attributes;

namespace Perlang.Stdlib
{
    /// <summary>
    /// Provides support for calling standard POSIX methods.
    ///
    /// The method names are deliberately using "POSIX-style", i.e. short, C-oriented names for methods like `getuid`,
    /// `getpid` and so forth. This is to make it simple for people with experience from POSIX-based systems to find the
    /// method they are looking for.
    /// </summary>
    /// <remarks>
    /// The XML method descriptions are based on [the NetBSD source code](https://github.com/NetBSD/src). The full
    /// license of these man pages can be found below.
    ///
    /// There might be subtle differences between systems on some of these methods. For information on how these work on
    /// e.g. Linux, use a command like `man 2 getgid` (replace `getgid` with the name of the function you are interested
    /// in).
    ///
    ///
    /// Copyright (c) 1980, 1991, 1993
    /// The Regents of the University of California.  All rights reserved.
    ///
    /// Redistribution and use in source and binary forms, with or without
    /// modification, are permitted provided that the following conditions
    /// are met:
    /// 1. Redistributions of source code must retain the above copyright
    ///    notice, this list of conditions and the following disclaimer.
    /// 2. Redistributions in binary form must reproduce the above copyright
    ///    notice, this list of conditions and the following disclaimer in the
    ///    documentation and/or other materials provided with the distribution.
    /// 3. Neither the name of the University nor the names of its contributors
    ///    may be used to endorse or promote products derived from this software
    ///    without specific prior written permission.
    ///
    /// THIS SOFTWARE IS PROVIDED BY THE REGENTS AND CONTRIBUTORS ``AS IS'' AND
    /// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
    /// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
    /// ARE DISCLAIMED.  IN NO EVENT SHALL THE REGENTS OR CONTRIBUTORS BE LIABLE
    /// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
    /// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
    /// OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
    /// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
    /// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
    /// OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
    /// SUCH DAMAGE.
    /// </remarks>
    /// <footer>
    /// For more details on Linux-specific aspects regarding these methods, please consult the Linux man pages project:
    /// https://man7.org/linux/man-pages/dir_all_alphabetic.html/.
    /// </footer>
    [GlobalClass(PlatformID.Unix, PlatformID.MacOSX)]
    public static class Posix
    {
        // Internal class which contains the P/Invoke definitions, to avoid exposing them directly to our callers.
        private static class Internal
        {
            [DllImport("libc")]
            public static extern int getegid();

            [DllImport("libc")]
            public static extern int geteuid();

            [DllImport("libc")]
            public static extern int getgid();

            [DllImport("libc")]
            public static extern int getpid();

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
        /// Returns the process ID of the calling process.  The ID is guaranteed to be unique and it might be tempting
        /// to use it for constructing temporary file names. However, this is _NOT_ recommended since it is not secure.
        /// In C `mkstemp` can be used for this purpose. `mkstemp` is not yet available to use from Perlang, though.
        ///
        /// <see cref="getpid">getpid()</see> conform to ISO/IEC 9945-1:1990 (`POSIX.1`).
        /// </summary>
        /// <returns>The process ID of the calling process.</returns>
        /// <seealso cref="getppid"/>
        public static int getpid()
        {
            return Internal.getpid();
        }

        /// <summary>
        /// Returns the process ID of the parent of the calling process.
        ///
        /// <see cref="getpid">getpid()</see> conform to ISO/IEC 9945-1:1990 (`POSIX.1`).
        /// </summary>
        /// <returns>The parent process ID.</returns>
        /// <seealso cref="getpid"/>
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
