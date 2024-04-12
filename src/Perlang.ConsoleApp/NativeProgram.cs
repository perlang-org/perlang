#nullable enable
#pragma warning disable SA1300
#pragma warning disable S4200
using System.Runtime.InteropServices;

namespace Perlang.ConsoleApp;

/// <summary>
/// Method definitions for native (Perlang) implementation of <see cref="Program"/>-related methods.
/// </summary>
internal static partial class NativeProgram
{
    /// <summary>
    /// Entry point for the native implementation of the Perlang CLI.
    /// </summary>
    /// <param name="argc">The number of command line arguments.</param>
    /// <param name="argv">The command line arguments.</param>
    [LibraryImport("perlang_cli", EntryPoint = "native_main", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void native_main(int argc, string[] argv);
}
