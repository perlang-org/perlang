#nullable enable
namespace Perlang.Interpreter.Resolution
{
    /// <summary>
    /// A binding to a (local or global) variable.
    /// </summary>
    internal class VariableBinding : Binding, IDistanceAwareBinding
    {
        public int Distance { get; }

        public override string ObjectType => "variable";
        public override bool IsMutable => true;

        public VariableBinding(ITypeReference? typeReference, int distance, Expr referringExpr)
            : base(typeReference, referringExpr)
        {
            Distance = distance;
        }
    }
}
