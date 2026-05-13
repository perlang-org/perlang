#nullable enable
#pragma warning disable SA1010
#pragma warning disable SA1115
#pragma warning disable SA1117
#pragma warning disable SA1118
using System.Collections.Immutable;

namespace Perlang.Stdlib;

public class LibcStdlibClass : IPerlangClass
{
    public static readonly LibcStdlibClass Instance = new();

    public string Name => "Libc";

    public ImmutableList<IPerlangFunction> Methods { get; } = ImmutableList.Create<IPerlangFunction>(
        new CppFunction("getcwd", ImmutableList<Parameter>.Empty, new TypeReference(PerlangTypes.String)),

        new CppFunction("getenv", [
                new Parameter(perlang_cli.CreateStringToken(TokenType.STRING, "name", "name", "", -1), typeReference: new TypeReference(PerlangTypes.String))
            ],
            new TypeReference(PerlangTypes.String)
        ),

        new CppFunction("getpid", ImmutableList<Parameter>.Empty, new TypeReference(PerlangValueTypes.Int32)) // pid_t is defined to be signed
    );

    public ImmutableList<IPerlangField> Fields => ImmutableList<IPerlangField>.Empty;
}
