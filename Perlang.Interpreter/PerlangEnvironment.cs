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

        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public PerlangEnvironment(IEnvironment enclosing = null)
        {
            // Cast is safe as long as we are the sole implementation of the IEnvironment interface.
            this.enclosing = (PerlangEnvironment)enclosing;
        }

        public void Define(string name, object value)
        {
            values[name] = value;
        }

        public object GetAt(int distance, string name)
        {
            return Ancestor(distance).values.TryGetObjectValue(name);
        }

        public void AssignAt(int distance, Token name, object value)
        {
            Ancestor(distance).values[name.Lexeme] = value;
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
            return Get(name.Lexeme);
        }

        internal object Get(string name)
        {
            if (values.ContainsKey(name))
            {
                return values[name];
            }

            // Fall-back to the enclosing scope if the variable isn't found in the current scope.
            return enclosing?.Get(name);
        }

        internal void Assign(Token name, object value)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                values[name.Lexeme] = value;
                return;
            }

            if (enclosing != null)
            {
                enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, "Undefined variable '" + name.Lexeme + "'.");
        }
    }
}
