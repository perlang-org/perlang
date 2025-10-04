namespace Perlang.Interpreter.NameResolution;

// TODO: Might be able to get rid of this completely if we fix enums to be evaluated at an earlier stage.
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
