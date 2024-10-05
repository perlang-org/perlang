#nullable enable
using System;

namespace Perlang.Interpreter.NameResolution;

internal class NativeClassBindingFactory : IBindingFactory
{
    private readonly Type type;

    // The fact that this is a "native" class is an implementation detail. The end goal is that Perlang classes and
    // "native" classes should be indistinguishable; in fact, Perlang classes aim to become full CLR classes in the
    // long run.
    public string ObjectType => "class";

    public NativeClassBindingFactory(Type type)
    {
        this.type = type ?? throw new ArgumentException("type cannot be null");
    }

    public Binding CreateBinding(int distance, Expr referringExpr)
    {
        return new NativeClassBinding(referringExpr, type);
    }
}