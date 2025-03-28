namespace Perlang.Interpreter.NameResolution;

internal class FunctionBindingFactory : IBindingFactory
{
    public string ObjectType => "function";

    private readonly ITypeReference typeReference;
    private readonly Stmt.Function function;

    public FunctionBindingFactory(ITypeReference typeReference, Stmt.Function function)
    {
        this.typeReference = typeReference;
        this.function = function;
    }

    public Binding CreateBinding(int distance, Expr referringExpr) =>
        new FunctionBinding(function, typeReference, distance, referringExpr);
}
