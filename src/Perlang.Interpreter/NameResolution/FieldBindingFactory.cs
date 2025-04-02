namespace Perlang.Interpreter.NameResolution;

internal class FieldBindingFactory : IBindingFactory
{
    public string ObjectType => "field";

    public Stmt.Class Class { get; }

    private readonly Stmt.Field field;
    private readonly bool isMutable;

    // TODO: Add a Class property to the Field class, so that each field knows what Class it belongs to. That way, we
    // TODO: can get rid of the Stmt.Class parameter to the methods here.
    public FieldBindingFactory(Stmt.Class @class, Stmt.Field field)
    {
        this.Class = @class;
        this.field = field;
        this.isMutable = field.IsMutable;
    }

    public Binding CreateBinding(Expr referringExpr) =>
        new FieldBinding(Class, field, referringExpr, isMutable);

    public static Binding CreateBindingForField(Stmt.Class @class, Stmt.Field field, Expr.Assign referringExpr) =>
        new FieldBinding(@class, field, referringExpr, field.IsMutable);
}
