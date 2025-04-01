namespace Perlang.Interpreter.NameResolution;

internal class EnumBindingFactory : IBindingFactory
{
    private readonly PerlangEnum perlangEnum;

    public string ObjectType => "enum";

    public EnumBindingFactory(PerlangEnum perlangEnum)
    {
        this.perlangEnum = perlangEnum;
    }

    public Binding CreateBinding(Expr referringExpr)
    {
        return new EnumBinding(referringExpr, perlangEnum);
    }
}
