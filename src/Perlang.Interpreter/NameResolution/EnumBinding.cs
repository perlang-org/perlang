#nullable enable

namespace Perlang.Interpreter.NameResolution;

internal class EnumBinding : Binding
{
    public PerlangEnum PerlangEnum { get; }

    public override string ObjectType => "enum";

    public EnumBinding(Expr referringExpr, PerlangEnum perlangEnum)
        : base(new TypeReference(perlangEnum.NameToken), referringExpr)
    {
        this.PerlangEnum = perlangEnum;
    }
}
