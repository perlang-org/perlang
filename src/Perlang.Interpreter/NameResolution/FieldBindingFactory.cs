namespace Perlang.Interpreter.NameResolution;

internal class FieldBindingFactory : IBindingFactory
{
    public string ObjectType => "field";

    private readonly ITypeReference typeReference;
    private readonly Stmt.Field field;
    private readonly bool isMutable;

    public FieldBindingFactory(ITypeReference typeReference, Stmt.Field field)
    {
        this.typeReference = typeReference;
        this.field = field;
        this.isMutable = field.IsMutable;
    }

    public Binding CreateBinding(int distance, Expr referringExpr) =>
        new FieldBinding(typeReference, referringExpr, isMutable);
}
