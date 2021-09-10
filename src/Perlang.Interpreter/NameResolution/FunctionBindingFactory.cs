namespace Perlang.Interpreter.NameResolution
{
    internal class FunctionBindingFactory : IBindingFactory
    {
        public ITypeReference TypeReference { get; }
        public Stmt.Function Function { get; }

        public string ObjectType => "function";

        public FunctionBindingFactory(ITypeReference typeReference, Stmt.Function function)
        {
            TypeReference = typeReference;
            Function = function;
        }

        public Binding CreateBinding(int distance, Expr referringExpr) =>
            new FunctionBinding(Function, TypeReference, distance, referringExpr);
    }
}
