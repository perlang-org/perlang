#nullable enable

namespace Perlang.Interpreter.NameResolution;

/// <summary>
/// A binding to a (local or global) variable.
/// </summary>
internal class VariableBinding : Binding
{
    public override string ObjectType => "variable";

    // TODO: Support immutable variables, and make them be immutable by default. We just need to think through what the
    // TODO: syntax should look like for mutable and immutable variables.
    protected override bool IsMutable => true;

    public VariableBinding(ITypeReference? typeReference, Expr referringExpr)
        : base(typeReference, referringExpr)
    {
    }
}
