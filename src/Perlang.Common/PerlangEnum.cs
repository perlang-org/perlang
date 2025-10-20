#pragma warning disable SA1010

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Perlang;

public class PerlangEnum : IPerlangType
{
    public Token NameToken { get; }
    public string Name => perlang_cli.GetTokenLexeme(NameToken);
    public bool IsEnum => true;
    public ImmutableList<IPerlangFunction> Methods { get; } = [];
    public ImmutableList<IPerlangField> Fields { get; } = [];
    public Dictionary<string, Expr> EnumMembers { get; }

    public PerlangEnum(IToken name, Dictionary<string, Expr> enumMembers)
    {
        NameToken = name as Token ?? throw new ArgumentException($"Internal error: only Token names are supported, not {name.GetType()}");
        EnumMembers = enumMembers;
    }
}
