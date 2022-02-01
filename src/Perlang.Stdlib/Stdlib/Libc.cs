#nullable enable
#pragma warning disable SA1300
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Perlang.Attributes;

namespace Perlang.Stdlib
{
    /// <summary>
    /// Provides support for calling standard C library functions.
    ///
    /// This class supplements see <see cref="Posix"/> class by implementing support for functions available on all
    /// supported platforms.
    /// </summary>
    /// <remarks>
    /// The XML method descriptions are based on [the .NET source code](https://github.com/dotnet/runtime), licensed
    /// under the MIT license. Copyright (c) .NET Foundation and Contributors. Small portions may also be based on [the
    /// NetBSD source code](https://github.com/NetBSD/src). The full license of these man pages can be found at
    /// https://github.com/perlang-org/perlang/blob/master/NOTICE.md
    /// </remarks>
    [GlobalClass]
    public static class Libc
    {
        // Internal class which contains the P/Invoke definitions, to avoid exposing them directly to our callers.
        [EditorBrowsable(EditorBrowsableState.Never)]
        private static class Internal
        {
#if _WINDOWS
            [DllImport("ucrtbase", EntryPoint = "_getpid")]
#else // POSIX
            [DllImport("libc")]
#endif
            public static extern int getpid();
        }

        /// <summary>Gets the fully qualified path of the current working directory.</summary>
        /// <value>The directory path.</value>
        /// <remarks>
        /// <format type="text/markdown"><![CDATA[
        /// ## Remarks
        /// By definition, if this process starts in the root directory of a local or network drive, the value of this
        /// property is the drive name followed by a trailing slash (for example, "C:\\"). If this process starts in a
        /// subdirectory, the value of this property is the drive and subdirectory path, without a trailing slash (for
        /// example, "C:\mySubDirectory").
        /// ]]></format>
        /// </remarks>
        /// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">A component of the pathname no longer exists.</exception>
        /// <returns>The current working directory.</returns>
        public static string getcwd()
        {
            return Environment.CurrentDirectory;
        }

        public static int getpid()
        {
            return Internal.getpid();
        }
    }
}
