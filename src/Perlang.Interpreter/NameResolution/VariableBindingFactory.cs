namespace Perlang.Interpreter.NameResolution
{
    /// <summary>
    /// Container class which supports creating <see cref="VariableBinding"/> instances.
    /// </summary>
    internal class VariableBindingFactory : IBindingFactory
    {
        /// <summary>
        /// Gets a `None` instance, indicating that no type information is available. Note that this is different from
        /// an unresolved type reference, which is the state of a TypeReference before type inference has taken place.
        ///
        /// It is also different from a `null` value, since `null` values are sometimes used to indicate that no
        /// variable with a given name could be find in the current scope, or any of its ancestors.
        /// </summary>
        public static VariableBindingFactory None { get; } = new VariableBindingFactory(null);

        public string ObjectType => "variable";

        public ITypeReference TypeReference { get; }

        public VariableBindingFactory(ITypeReference typeReference)
        {
            TypeReference = typeReference;
        }

        public Binding CreateBinding(int distance, Expr referringExpr) =>
            new VariableBinding(TypeReference, distance, referringExpr);
    }
}
