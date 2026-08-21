#nullable enable

using System;

namespace Perlang.Interpreter.NameResolution;

// TODO: Change to TypeBindingFactory
internal class InterfaceBindingFactory : IBindingFactory
{
    private readonly IPerlangType perlangInterface;
    private readonly TypeReference typeReference;

    public string ObjectType => "class";

    public InterfaceBindingFactory(IPerlangType perlangInterface, TypeReference typeReference)
    {
        this.perlangInterface = perlangInterface ?? throw new ArgumentException("perlangInterface cannot be null");
        this.typeReference = typeReference ?? throw new ArgumentException("typeReference cannot be null");
    }

    public Binding CreateBinding(Expr referringExpr)
    {
        return new TypeBinding(referringExpr, perlangInterface, "interface", typeReference);
    }
}
