#nullable enable

namespace Perlang.Interpreter.NameResolution;

/// <summary>
/// A ClassBinding is a binding to a Perlang class. Note that this is specifically not referring to an instance of
/// a class, but to the class itself.
/// </summary>
internal class ClassBinding : Binding
{
    public IPerlangClass? PerlangClass { get; }

    public override string ObjectType => "class";

    public ClassBinding(Expr referringExpr, IPerlangClass? perlangClass)
        : base(new TypeReference(typeof(IPerlangClass)), referringExpr)
    {
        PerlangClass = perlangClass;
    }
}
