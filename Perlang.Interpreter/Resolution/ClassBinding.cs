namespace Perlang.Interpreter.Resolution
{
    internal class ClassBinding : Binding
    {
        public PerlangClass PerlangClass { get; }

        public ClassBinding(TypeReference typeReference, Expr referringExpr, PerlangClass perlangClass) :
            base(typeReference, referringExpr)
        {
            PerlangClass = perlangClass;
        }
    }
}
