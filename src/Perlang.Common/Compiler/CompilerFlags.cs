#pragma warning disable S2344
using System;

namespace Perlang.Compiler;

[Flags]
public enum CompilerFlags
{
    /// <summary>
    /// No compiler flags have been specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// This flag disables all caching of compiled Perlang code, to ensure that all parts of the compilation is being
    /// used.
    /// </summary>
    CacheDisabled = 1,

    /// <summary>
    /// This flag which makes the compiler remove the `main` method if it is empty. This can be useful e.g. when the
    /// `main` method is implemented in C++.
    /// </summary>
    RemoveEmptyMainMethod = 2,

    /// <summary>
    /// This flag makes the compiler avoid writing timestamps and the Perlang compiler version to generated C++ files.
    /// This is useful when these files are committed to version control, to avoid redundant diffs.
    /// </summary>
    Idempotent = 4
}
