#nullable enable
namespace Perlang.Interpreter.NameResolution;

/// <summary>
/// A TypeBinding is a binding to a Perlang type. Note that for classes, this is specifically not referring to an
/// instance of a class, but to the class itself.
/// </summary>
internal class TypeBinding : Binding
{
    public IPerlangType PerlangType { get; }
    public TypeReference ClassTypeReference { get; }

    public override string ObjectType { get; }

    public TypeBinding(Expr referringExpr, IPerlangType perlangType, string objectType, TypeReference classTypeReference)
        : base(classTypeReference, referringExpr)
    {
        PerlangType = perlangType;
        ObjectType = objectType;
        ClassTypeReference = classTypeReference;
    }
}
