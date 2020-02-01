using System.Collections.Generic;
using Perlang.Interpreter.Extensions;

namespace Perlang.Interpreter
{
    /// <summary>
    /// Holds information about the context for a static scope (functions and variable name bindings).
    /// </summary>
    public class PerlangEnvironment
    {
        private readonly PerlangEnvironment enclosing;

        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public PerlangEnvironment(PerlangEnvironment enclosing = null)
        {
            this.enclosing = enclosing;
        }

        internal void Define(string name, object value)
        {
            values[name] = value;
        }

        internal object GetAt(int distance, string name)
        {
            return Ancestor(distance).values.TryGetObjectValue(name);
        }

        internal void AssignAt(int distance, Token name, object value)
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
