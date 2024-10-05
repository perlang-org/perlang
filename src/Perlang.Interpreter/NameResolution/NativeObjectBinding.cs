using System;

namespace Perlang.Interpreter.NameResolution;

/// <summary>
/// Binding implementation for referring to native .NET objects.
/// </summary>
internal class NativeObjectBinding : Binding
{
    public Type Type { get; }

    // This refers to an instance of a native class, i.e. an "object".
    public override string ObjectType => "object";

    internal NativeObjectBinding(Expr referringExpr, Type type)
        : base(new TypeReference(type), referringExpr)
    {
        Type = type;
    }
}