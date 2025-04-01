#nullable enable

using Perlang.Interpreter.NameResolution;

namespace Perlang.Interpreter.Internals
{
    public interface IBindingHandler : IBindingRetriever
    {
        /// <summary>
        /// Adds an expression to the global scope.
        /// </summary>
        /// <param name="binding">The binding to add.</param>
        void AddGlobalExpr(Binding binding);

        /// <summary>
        /// Adds an expression to a local scope at a given depth away from the call site. One level of nesting = one
        /// extra level of depth.
        /// </summary>
        /// <param name="binding">The binding to add.</param>
        void AddLocalExpr(Binding binding);
    }
}
