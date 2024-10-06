#nullable enable

namespace Perlang.Interpreter.NameResolution;

internal class EnumBinding : Binding
{
    public PerlangEnum PerlangEnum { get; set; }

    public override string ObjectType => "enum";

    public EnumBinding(Expr referringExpr, PerlangEnum perlangEnum)
        : base(new TypeReference(typeof(PerlangEnum)), referringExpr)
    {
        this.PerlangEnum = perlangEnum;
    }
}
