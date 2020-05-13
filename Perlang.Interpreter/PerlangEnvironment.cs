using System.Collections.Generic;
using Perlang.Interpreter.Extensions;

namespace Perlang.Interpreter
{
    /// <summary>
    /// Holds information about the context for a static scope (functions and variable name bindings).
    /// </summary>
    internal class PerlangEnvironment : IEnvironment
    {
        private readonly PerlangEnvironment enclosing;

        private readonly Dictionary<string, Binding> values = new Dictionary<string, Binding>();

        public PerlangEnvironment(IEnvironment enclosing = null)
        {
            // Cast is safe as long as we are the sole implementation of the IEnvironment interface.
            this.enclosing = (PerlangEnvironment)enclosing;
        }

        /// <summary>
        /// Define a binding from a name to a value. This is used both for functions and local/global variables.
        /// </summary>
        /// <param name="name">The name of the binding to create.</param>
        /// <param name="expr">The expression to which this binding belongs. Should be null for statements.</param>
        /// <param name="value">The value. Can be null.</param>
        public void Define(string name, Expr expr, object value)
        {
            values[name] = new Binding(expr, value);
        }

        public IBinding GetAt(int distance, string name)
        {
            return Ancestor(distance).values.TryGetObjectValue(name);
        }

        public void AssignAt(int distance, Token name, Expr expr, object value)
        {
            Ancestor(distance).values[name.Lexeme] = new Binding(expr, value);
        }

        private PerlangEnvironment Ancestor(int distance)
        {
            PerlangEnvironment environment = this;

            for (int i = 0; i < distance; i++)
            {
                environment = environment.enclosing;
            }

            return environment;
        }

        internal object Get(Token name)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                return values[name.Lexeme];
            }

            // Fall-back to the enclosing scope if the variable isn't found in the current scope.
            if (enclosing != null)
            {
                return enclosing.Get(name);
            }

            throw new RuntimeError(name, "Undefined variable '" + name.Lexeme + "'.");
        }

        internal void Assign(Token name, Expr expr, object value)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                values[name.Lexeme] = new Binding(expr, value);
                return;
            }

            if (enclosing != null)
            {
                enclosing.Assign(name, expr, value);
                return;
            }

            throw new RuntimeError(name, "Undefined variable '" + name.Lexeme + "'.");
        }
    }
}
