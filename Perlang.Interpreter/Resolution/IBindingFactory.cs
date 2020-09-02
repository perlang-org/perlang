namespace Perlang.Interpreter.Resolution
{
    internal interface IBindingFactory
    {
        Binding CreateBinding(int distance, Expr referringExpr);
    }
}
