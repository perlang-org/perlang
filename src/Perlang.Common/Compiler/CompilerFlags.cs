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
    CacheDisabled = 1
}
