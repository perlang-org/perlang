namespace Perlang.Interpreter.Resolution
{
    internal class FunctionBindingFactory : IBindingFactory
    {
        public TypeReference TypeReference { get; }
        public Stmt.Function Function { get; }

        public string ObjectType => "function";

        public FunctionBindingFactory(TypeReference typeReference, Stmt.Function function)
        {
            TypeReference = typeReference;
            Function = function;
        }

        public Binding CreateBinding(int distance, Expr referringExpr) =>
            new FunctionBinding(Function, TypeReference, distance, referringExpr);
    }
}
