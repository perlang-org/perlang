namespace Perlang.Interpreter.Resolution
{
    /// <summary>
    /// A binding to a function defined in Perlang.
    /// </summary>
    internal class FunctionBinding : Binding, IDistanceAwareBinding
    {
        public int Distance { get; }
        public Stmt.Function Function { get; }

        public override string ObjectType => "function";

        public FunctionBinding(Stmt.Function function, TypeReference typeReference, int distance, Expr referringExpr)
            : base(typeReference, referringExpr)
        {
            // Likewise, the Function property is permitted to be null for variable bindings (but not for Call
            // bindings)
            if (referringExpr is Expr.Call)
            {
                Function = function ?? throw new PerlangInterpreterException(
                    "When referringExpr is an Expr.Call instance, function cannot be null");
            }

            Distance = distance;
        }
    }
}
