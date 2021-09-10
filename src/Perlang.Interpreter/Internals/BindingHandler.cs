#nullable enable

using System.Collections.Generic;
using Perlang.Interpreter.NameResolution;

namespace Perlang.Interpreter.Internals
{
    /// <summary>
    /// Class responsible for handling global and local bindings.
    /// </summary>
    internal class BindingHandler : IBindingHandler
    {
        /// <summary>
        /// Map from referring expression to global binding (variable or function).
        /// </summary>
        private readonly IDictionary<Expr, Binding> globalBindings = new Dictionary<Expr, Binding>();

        /// <summary>
        /// Map from referring expression to local binding (i.e. in a local scope) for variable or function.
        /// </summary>
        private readonly IDictionary<Expr, Binding> localBindings = new Dictionary<Expr, Binding>();

        public void AddGlobalExpr(Binding binding)
        {
            globalBindings[binding.ReferringExpr] = binding;
        }

        public void AddLocalExpr(Binding binding)
        {
            localBindings[binding.ReferringExpr] = binding;
        }

        public bool GetLocalBinding(Expr expr, out Binding? binding)
        {
            return localBindings.TryGetValue(expr, out binding);
        }

        public Binding? GetVariableOrFunctionBinding(Expr expr)
        {
            if (localBindings.ContainsKey(expr))
            {
                return localBindings[expr];
            }

            if (globalBindings.ContainsKey(expr))
            {
                return globalBindings[expr];
            }

            // The variable does not exist, neither in the list of local nor global bindings.
            return null;
        }
    }
}
