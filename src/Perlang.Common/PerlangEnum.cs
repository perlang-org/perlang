#pragma warning disable SA1010

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Perlang;

public class PerlangEnum : IPerlangType
{
    public Token NameToken { get; }
    public string Name => NameToken.Lexeme;
    public bool IsEnum => true;
    public ImmutableList<IPerlangFunction> Methods { get; } = [];
    public ImmutableList<IPerlangField> Fields { get; } = [];
    public Dictionary<string, Expr> EnumMembers { get; }

    public PerlangEnum(Token name, Dictionary<string, Expr> enumMembers)
    {
        NameToken = name;
        EnumMembers = enumMembers;
    }
}
