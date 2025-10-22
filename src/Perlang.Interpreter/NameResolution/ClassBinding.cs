#nullable enable

namespace Perlang.Interpreter.NameResolution;

/// <summary>
/// A ClassBinding is a binding to a Perlang class. Note that this is specifically not referring to an instance of
/// a class, but to the class itself.
/// </summary>
internal class ClassBinding : Binding
{
    public IPerlangClass PerlangClass { get; }
    public TypeReference ClassTypeReference { get; }

    public override string ObjectType => "class";

    public ClassBinding(Expr referringExpr, IPerlangClass perlangClass, TypeReference classTypeReference)
        : base(classTypeReference, referringExpr)
    {
        PerlangClass = perlangClass;
        ClassTypeReference = classTypeReference;
    }
}
