using System;

namespace Perlang.Interpreter.NameResolution;

internal class ClassBindingFactory : IBindingFactory
{
    private readonly PerlangClass perlangClass;

    public string ObjectType => "class";

    public ClassBindingFactory(PerlangClass perlangClass)
    {
        this.perlangClass = perlangClass ?? throw new ArgumentException("perlangClass cannot be null");
    }

    public Binding CreateBinding(int distance, Expr referringExpr)
    {
        return new ClassBinding(referringExpr, perlangClass);
    }
}
