using System;
using System.Collections.Generic;
using Perlang.Interpreter.NameResolution;

namespace Perlang.Interpreter.Typing
{
    /// <summary>
    /// Base class for classes which perform type validation.
    /// </summary>
    internal class Validator : VisitorBase
    {
        private protected Func<Expr, Binding> GetVariableOrFunctionCallback { get; }
        private protected Action<TypeValidationError> TypeValidationErrorCallback { get; }

        protected Validator(
            Func<Expr, Binding> getVariableOrFunctionCallback,
            Action<TypeValidationError> typeValidationErrorCallback)
        {
            GetVariableOrFunctionCallback = getVariableOrFunctionCallback;
            TypeValidationErrorCallback = typeValidationErrorCallback;
        }

        /// <summary>
        /// Validates the given set of statements, reporting errors to the defined callbacks.
        /// </summary>
        /// <param name="statements">A list of <see cref="Stmt"/> statements.</param>
        public void ReportErrors(IList<Stmt> statements)
        {
            foreach (Stmt stmt in statements)
            {
                stmt.Accept(this);
            }
        }
    }
}
