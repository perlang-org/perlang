namespace Perlang.Interpreter.Resolution
{
    /// <summary>
    /// Holds information about a binding.
    ///
    /// Different types of bindings (=subclasses) provide slightly different mechanisms to retrieve information about
    /// the variable or function being bound to.
    /// </summary>
    internal abstract class Binding
    {
        /// <summary>
        /// The type reference of the declaring statement. (typically a 'var' initializer or a function return type)
        /// </summary>
        public TypeReference TypeReference { get; }

        /// <summary>
        /// An expression referring to the declaring statement's type reference. Note that multiple expressions can
        /// refer to a single declaration statement, as illustrated by the following program:
        ///
        /// var a = 123;
        /// var b = a; // b refers to a
        /// var c = a; // c also refers to a
        /// </summary>
        public Expr ReferringExpr { get; }

        protected Binding(TypeReference typeReference, Expr referringExpr)
        {
            // We allow null references on this one to sneak through, since it allows this test to succeed:
            // Perlang.Tests.Var.VarTests.use_local_in_initializer
            // Future improvements in this area would be to use something else than 'null' to indicate this, e.g.
            // TypeReference.None
            TypeReference = typeReference;

            ReferringExpr = referringExpr ?? throw new PerlangInterpreterException("referringExpr cannot be null");
        }
    }
}
