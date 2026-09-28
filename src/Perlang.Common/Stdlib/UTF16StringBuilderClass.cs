#nullable enable
#pragma warning disable SA1010
#pragma warning disable SA1115
#pragma warning disable SA1117
#pragma warning disable SA1118
#pragma warning disable S101
using System.Collections.Immutable;

namespace Perlang.Stdlib;

public class UTF16StringBuilderClass : IPerlangClass
{
    public static readonly UTF16StringBuilderClass Instance = new();

    public string Name => "UTF16StringBuilder";

    public ImmutableList<IPerlangFunction> Methods { get; } = ImmutableList.Create<IPerlangFunction>(
        new CppFunction("append", [
                new Parameter(perlang_cli.CreateStringToken(TokenType.STRING, "c", "c", "", -1), new TypeReference(PerlangValueTypes.Char))
            ],
            new TypeReference(PerlangValueTypes.Void)
        ),

        // TODO: Enable once the compiler supports method overloads
        // (https://gitlab.perlang.org/perlang/perlang/-/work_items/527). Until then, only one 'append' overload can be
        // registered.
        //
        // new CppFunction("append", [
        //         new Parameter(perlang_cli.CreateStringToken(TokenType.STRING, "str", "str", "", -1), new TypeReference(PerlangTypes.String))
        //     ],
        //     new TypeReference(PerlangValueTypes.Void)
        // ),

        new CppFunction("append_line", [
                new Parameter(perlang_cli.CreateStringToken(TokenType.STRING, "str", "str", "", -1), new TypeReference(PerlangTypes.String))
            ],
            new TypeReference(PerlangValueTypes.Void)
        ),

        new CppFunction("length", ImmutableList<Parameter>.Empty, new TypeReference(PerlangValueTypes.UInt64)),
        new CppFunction("to_string", ImmutableList<Parameter>.Empty, new TypeReference(PerlangTypes.String))
    );

    public ImmutableList<IPerlangField> Fields => [];
}
