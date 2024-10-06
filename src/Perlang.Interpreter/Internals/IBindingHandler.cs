#nullable enable

using Perlang.Interpreter.NameResolution;

namespace Perlang.Interpreter.Internals
{
    public interface IBindingHandler
    {
        Binding? GetVariableOrFunctionBinding(Expr expr);

        /// <summary>
        /// Adds an expression to the global scope.
        /// </summary>
        /// <param name="binding">The binding to add.</param>
        void AddGlobalExpr(Binding binding);

        /// <summary>
        /// Adds an expression to a local scope at a given depth away from the call site. One level of nesting = one
        /// extra level of depth. The depth is tracked in e.g. <see cref="VariableBinding.Distance"/> for variable
        /// bindings.
        /// </summary>
        /// <param name="binding">The binding to add.</param>
        void AddLocalExpr(Binding binding);
    }
}
