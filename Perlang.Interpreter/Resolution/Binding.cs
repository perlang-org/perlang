using System.Diagnostics.CodeAnalysis;

#nullable enable
namespace Perlang.Interpreter.Resolution
{
    /// <summary>
    /// Holds information about a binding.
    ///
    /// Different types of bindings (=subclasses) provide slightly different mechanisms to retrieve information about
    /// the variable, function or class being bound to.
    /// </summary>
    internal abstract class Binding
    {
        /// <summary>
        /// Gets the type reference of the declaring statement (typically a 'var' initializer or a function return type).
        /// </summary>
        public TypeReference? TypeReference { get; }

        /// <summary>
        /// Gets an expression referring to the declaring statement's type reference. Note that multiple expressions can
        /// refer to a single declaration statement, as illustrated by the following program:
        ///
        /// ```c#
        /// var a = 123;
        /// var b = a; // b refers to a
        /// var c = a; // c also refers to a.
        /// ```
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1629:DocumentationTextMustEndWithAPeriod", Justification = "Code block.")]
        public Expr ReferringExpr { get; }

        protected Binding(TypeReference? typeReference, Expr referringExpr)
        {
            // We allow null references on this one to sneak through, since it allows this test to succeed:
            // Perlang.Tests.Integration.Classes.ClassesTests.can_call_static_method
            // Future improvements in this area would be to use something else than 'null' to indicate this, e.g.
            // TypeReference.None
            TypeReference = typeReference;

            ReferringExpr = referringExpr ?? throw new PerlangInterpreterException("referringExpr cannot be null");
        }
    }
}
