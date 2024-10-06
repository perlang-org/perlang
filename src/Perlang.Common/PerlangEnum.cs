using System.Collections.Generic;

namespace Perlang;

public class PerlangEnum
{
    public Token Name { get; }
    public Dictionary<string, Expr> EnumMembers { get; }

    public PerlangEnum(Token name, Dictionary<string, Expr> enumMembers)
    {
        Name = name;
        EnumMembers = enumMembers;
    }
}
