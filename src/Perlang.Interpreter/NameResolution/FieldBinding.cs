namespace Perlang.Interpreter.NameResolution;

internal class FieldBinding : Binding
{
    protected override bool IsMutable { get; }
    public override string ObjectType => "field";

    public FieldBinding(ITypeReference typeReference, Expr referringExpr, bool isMutable)
        : base(typeReference, referringExpr)
    {
        IsMutable = isMutable;
    }
}
