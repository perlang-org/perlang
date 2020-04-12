namespace Perlang.Interpreter.Resolution
{
    /// <summary>
    /// Container class for a TypeReference and optionally a function.
    /// </summary>
    internal class TypeReferenceAndFunction
    {
        /// <summary>
        /// A "None" instance, indicating that no type information is available. Note that this is different
        /// from an unresolved type reference, which is the state of a TypeReference before type inference has taken
        /// place.
        ///
        /// It is also different from a null value, since null values are sometimes used to indicate that no variable
        /// with a given name could be find in the current scope, or any of its ancestors.
        /// </summary>
        public static TypeReferenceAndFunction None { get; } = new TypeReferenceAndFunction(null, null);

        public TypeReference TypeReference { get; }
        public Stmt.Function Function { get; }

        public TypeReferenceAndFunction(TypeReference typeReference, Stmt.Function function)
        {
            TypeReference = typeReference;
            Function = function;
        }
    }
}
