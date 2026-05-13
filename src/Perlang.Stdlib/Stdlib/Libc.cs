#nullable enable
#pragma warning disable SA1601
#pragma warning disable SA1300
#pragma warning disable SA1310
#pragma warning disable S3218
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Perlang.Lang;
using String = System.String;

namespace Perlang.Stdlib;

/// <summary>
/// Provides support for calling standard C library functions.
///
/// This class supplements the `Posix` class by implementing support for functions available on all
/// supported platforms.
/// </summary>
/// <remarks>
/// The XML method descriptions are based on [the .NET source
/// code](https://github.com/dotnet/dotnet-api-docs/blob/main/xml/System/Environment.xml), licensed
/// under the MIT license. Copyright (c) .NET Foundation and Contributors. Some method descriptions are also based
/// on `man` pages in [the NetBSD source code](https://github.com/NetBSD/src). The full license of these man pages
/// can be found at https://gitlab.perlang.org/perlang/perlang/-/blob/master/NOTICE.md.
///
/// Portions may also be inspired by Donald Lewine's great book "POSIX Programmer's Guide" (O'Reilly 1991).
/// </remarks>
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
        internal const int STDOUT_FILENO = 1;
        internal const int STDERR_FILENO = 2;

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
}
