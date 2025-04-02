namespace Perlang.Interpreter.NameResolution;

internal class FieldBinding : Binding
{
    protected override bool IsMutable { get; }
    public override string ObjectType => "field";

    public Stmt.Class Class { get; }
    public Stmt.Field Field { get; }

    public FieldBinding(Stmt.Class @class, Stmt.Field field, Expr referringExpr, bool isMutable)
        : base(field.TypeReference, referringExpr)
    {
        Class = @class;
        Field = field;
        IsMutable = isMutable;
    }
}
