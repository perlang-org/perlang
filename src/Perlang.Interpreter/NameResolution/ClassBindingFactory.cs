using System;

namespace Perlang.Interpreter.NameResolution;

internal class ClassBindingFactory : IBindingFactory
{
    private readonly IPerlangClass perlangClass;
    private readonly TypeReference typeReference;

    public string ObjectType => "class";

    public ClassBindingFactory(IPerlangClass perlangClass, TypeReference typeReference)
    {
        this.perlangClass = perlangClass ?? throw new ArgumentException("perlangClass cannot be null");
        this.typeReference = typeReference ?? throw new ArgumentException("typeReference cannot be null");
    }

    public Binding CreateBinding(Expr referringExpr)
    {
        return new ClassBinding(referringExpr, perlangClass, typeReference);
    }
}
