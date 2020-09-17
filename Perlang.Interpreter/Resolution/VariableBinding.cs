#nullable enable
namespace Perlang.Interpreter.Resolution
{
    /// <summary>
    /// A binding to a variable (local or global)
    /// </summary>
    internal class VariableBinding : Binding, IDistanceAwareBinding
    {
        public int Distance { get; }

        public VariableBinding(TypeReference? typeReference, int distance, Expr referringExpr) :
            base(typeReference, referringExpr)
        {
            Distance = distance;
        }
    }
}
