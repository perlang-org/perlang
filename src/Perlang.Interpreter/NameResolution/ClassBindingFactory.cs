using System;

namespace Perlang.Interpreter.NameResolution;

internal class ClassBindingFactory : IBindingFactory
{
    private readonly IPerlangClass perlangClass;

    public string ObjectType => "class";

    public ClassBindingFactory(IPerlangClass perlangClass)
    {
        this.perlangClass = perlangClass ?? throw new ArgumentException("perlangClass cannot be null");
    }

    public Binding CreateBinding(Expr referringExpr)
    {
        return new ClassBinding(referringExpr, perlangClass);
    }
}
