#nullable enable
#pragma warning disable SA1601
#pragma warning disable SA1300
#pragma warning disable S3218
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Perlang.Attributes;
using Perlang.Lang;
using String = System.String;

namespace Perlang.Stdlib
{
    /// <summary>
    /// Provides support for calling standard C library functions.
    ///
    /// This class supplements see <see cref="Posix"/> class by implementing support for functions available on all
    /// supported platforms.
    /// </summary>
    /// <remarks>
    /// The XML method descriptions are based on [the .NET source
    /// code](https://github.com/dotnet/dotnet-api-docs/blob/main/xml/System/Environment.xml), licensed
    /// under the MIT license. Copyright (c) .NET Foundation and Contributors. Some method descriptions are also based
    /// on `man` pages in [the NetBSD source code](https://github.com/NetBSD/src). The full license of these man pages
    /// can be found at https://github.com/perlang-org/perlang/blob/master/NOTICE.md.
    ///
    /// Portions may also be inspired by Donald Lewine's great book "POSIX Programmer's Guide" (O'Reilly 1991).
    /// </remarks>
    [GlobalClass]
    public static partial class Libc
    {
        /// <summary>
        /// Contains P/Invoke definitions for libc methods.
        ///
        /// This class is deliberately internal, to avoid exposing these methods to code outside of this assembly.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static partial class Internal
        {
#if _WINDOWS
            [DllImport("ucrtbase", EntryPoint = "_getpid")]
#else // POSIX
            [DllImport("libc")]
#endif
            public static extern int getpid();

#if _WINDOWS
            [LibraryImport("ucrtbase")]
#else // POSIX
            [LibraryImport("libc")]
#endif
            internal static unsafe partial int memcmp(byte* b1, byte* b2, uint count);

#if _WINDOWS
            [LibraryImport("ucrtbase")]
#else // POSIX
            [LibraryImport("libc")]
#endif
            internal static partial int toupper(int c);
        }

        /// <summary>Gets the fully qualified path of the current working directory.</summary>
        /// <value>The directory path.</value>
        /// <remarks>
        /// By definition, if this process starts in the root directory of a local or network drive, the value of this
        /// property is the drive name followed by a trailing slash (for example, `/` or `C:\\`). If this process starts in a
        /// subdirectory, the value of this property is the drive and subdirectory path, without a trailing slash (for
        /// example, `/usr/bin` or `C:\mySubDirectory`).
        /// </remarks>
        /// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">A component of the pathname no longer exists.</exception>
        /// <returns>The current working directory.</returns>
        public static string getcwd()
        {
            return Environment.CurrentDirectory;
        }

        /// <summary>Retrieves all environment variable names and their values from the current process.</summary>
        /// <returns>A dictionary that contains all environment variable names and their values; otherwise, an empty
        /// dictionary if no environment variables are found.</returns>
        /// <remarks>
        /// The names and values for the environment variables are stored as key-value pairs in the returned
        /// <see cref="ImmutableDictionary"/>. Note that this dictionary is **case-sensitive** when looking up items
        /// based on the key, even on Windows.
        ///
        /// ### On Linux and macOS systems
        ///
        /// On Linux and macOS, the <see cref="environ"/> method retrieves the name and value of all environment
        /// variables that are inherited from the parent process that launched the `dotnet` process or that are defined
        /// within the scope of the `dotnet` process itself. Once the `dotnet` process ends, these latter environment
        /// variables cease to exist.
        ///
        /// .NET running on Unix-based systems does not support per-machine or per-user environment variables.
        ///
        /// ### On Windows systems
        ///
        /// On Windows systems, the <see cref="environ"/> method returns the following environment variables:
        ///
        /// - All per-machine environment variables that are defined at the time the process is created, along with
        ///   their values.
        ///
        /// - All per-user environment variables that are defined at the time the process is created, along with their
        ///   values.
        ///
        /// - Any variables inherited from the parent process from which the application was launched.
        ///
        /// ### Standard environment variables
        ///
        /// The POSIX standard defines the following environment variables:
        ///
        /// * `HOME`: The name of the user's initial working directory.
        /// * `LANG`: The name of the predefined setting for locale.
        /// * `LC_ALL`: The default locale to use if any of the following `LC_` symbols is not defined.
        /// * `LC_COLLATE`: The name of the locale for collation information.
        /// * `LC_CTYPE`: The name of the locale for character classification.
        /// * `LC_MONETARY`: The name of the locale for monetary related information.
        /// * `LC_NUMERIC`: The name of the locale for numeric editing.
        /// * `LC_TIME`: The name of the locale for date- and time-formatting information.
        /// * `LOGNAME`: The name of the user's login account.
        /// * `PATH`: The sequence of path prefixes used by "exec" functions in locating programs to run.
        /// * `TERM`: The user's terminal type.
        /// * `TZ`: Time zone information.
        /// </remarks>
        public static ImmutableDictionary<Lang.String, string> environ()
        {
            var result = new Dictionary<Lang.String, string>();

            foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                result.Add(AsciiString.from((string)entry.Key), (string?)entry.Value ?? String.Empty);
            }

            return result.ToImmutableDictionary();
        }

        /// <summary>Retrieves the value of an environment variable from the current process.</summary>
        /// <param name="name">The name of the environment variable.</param>
        /// <returns>The value of the environment variable specified by <paramref name="name" />, or <see
        /// langword="null" /> if the environment variable is not found.</returns>
        /// <remarks>
        /// The  <see cref="getenv(Lang.String)"/> method retrieves an environment
        /// variable from the environment block of the current process.
        ///
        /// To retrieve all environment variables along with their values, call the <see cref="environ"/> method.
        ///
        /// Environment variable names are case-sensitive on Linux and macOS and case-insensitive on Windows.
        ///
        /// ### On Linux and macOS systems
        ///
        /// On Linux and macOS, the environment block of the current process includes the following environment
        /// variables:
        ///
        /// - All environment variables that are provided to it by the parent process that created it. For .NET
        ///   applications launched from a shell, this includes all environment variables defined in the shell.
        ///
        /// .NET on Linux and macOS does not support per-machine or per-user environment variables.
        ///
        /// ### On Windows systems
        ///
        /// On Windows systems, the environment block of the current process includes:
        ///
        /// - All environment variables that are provided to it by the parent process that created it. For example, a
        ///   .NET application launched from a console window inherits all of the console window's environment
        ///   variables.
        ///
        ///       If there is no parent process, per-machine and per-user environment variables are used instead. For
        ///       example, a new console window has all per-machine and per-user environment variables defined at the
        ///       time it was launched.
        ///
        /// ### Standard environment variables
        ///
        /// In addition to user-defined environment variables, ISO/IEC 9945-1:1990 (`POSIX.1`) defines a set of standard
        /// environment variables. See <see cref="environ"/> for more details
        /// on these.
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="name" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission to
        /// perform this operation.</exception>
        public static string? getenv(Lang.String name)
        {
            return Environment.GetEnvironmentVariable(name.ToString());
        }

        /// <summary>
        /// Returns the process ID of the calling process.  The ID is guaranteed to be unique and it might be tempting
        /// to use it for constructing temporary file names. However, this is _NOT_ recommended since it is not secure.
        /// In C, `mkstemp` can be used for this purpose. `mkstemp` is not yet available to use from Perlang, though.
        ///
        /// <see cref="getpid">getpid()</see> conforms to ISO/IEC 9945-1:1990 (`POSIX.1`).
        /// </summary>
        /// <returns>The process ID of the calling process.</returns>
        /// <seealso cref="Posix.getppid"/>
        public static int getpid()
        {
            return Internal.getpid();
        }
    }
}
