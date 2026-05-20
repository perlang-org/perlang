#nullable enable
#pragma warning disable SA1010
#pragma warning disable SA1115
#pragma warning disable SA1117
using System.Collections.Immutable;

namespace Perlang.Stdlib;

/// <summary>
/// Perlang class definition for the <c>perlang::Posix</c> class.
/// </summary>
public class PosixStdlibClass : IPerlangClass
{
    public static readonly PosixStdlibClass Instance = new();

    public string Name => "Posix";

    public ImmutableList<IPerlangFunction> Methods { get; } = ImmutableList.Create<IPerlangFunction>(
        new CppFunction("getegid", ImmutableList<Parameter>.Empty, new TypeReference(PerlangValueTypes.UInt32)),
        new CppFunction("geteuid", ImmutableList<Parameter>.Empty, new TypeReference(PerlangValueTypes.UInt32)),
        new CppFunction("getgid",  ImmutableList<Parameter>.Empty, new TypeReference(PerlangValueTypes.UInt32)),
        new CppFunction("getppid", ImmutableList<Parameter>.Empty, new TypeReference(PerlangValueTypes.Int32)), // pid_t is defined to be signed
        new CppFunction("getuid",  ImmutableList<Parameter>.Empty, new TypeReference(PerlangValueTypes.UInt32))
    );

    public ImmutableList<IPerlangField> Fields => [];
}
