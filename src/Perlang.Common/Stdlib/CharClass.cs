#nullable enable
#pragma warning disable SA1010
#pragma warning disable SA1115
#pragma warning disable SA1117
#pragma warning disable SA1118
using System.Collections.Immutable;

namespace Perlang.Stdlib;

public class CharClass : IPerlangClass
{
    public static readonly CharClass Instance = new();

    public string Name => "Char";

    public ImmutableList<IPerlangFunction> Methods { get; } = ImmutableList.Create<IPerlangFunction>(
        new CppFunction("to_upper", [
                new Parameter(perlang_cli.CreateStringToken(TokenType.STRING, "literal", "literal", "", -1), typeReference: new TypeReference(PerlangValueTypes.Char))
            ],
            new TypeReference(PerlangValueTypes.Char)
        ),

        new CppFunction("to_lower", [
                new Parameter(perlang_cli.CreateStringToken(TokenType.STRING, "literal", "literal", "", -1), typeReference: new TypeReference(PerlangValueTypes.Char))
            ],
            new TypeReference(PerlangValueTypes.Char)
        )
    );

    public ImmutableList<IPerlangField> Fields => [];
}
