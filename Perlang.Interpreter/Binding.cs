namespace Perlang.Interpreter
{
    public class Binding : IBinding
    {
        public Expr Expr { get; }
        public object Value { get; }

        internal Binding(Expr expr, object value)
        {
            Expr = expr;
            Value = value;
        }
    }
}
