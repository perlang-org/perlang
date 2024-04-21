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
    /// This flag which makes the compiler remove the `main` method if it is empty. This
    /// can be useful e.g. when the `main` method is implemented in C++.
    /// </summary>
    RemoveEmptyMainMethod = 2
}
