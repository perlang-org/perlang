using System.Collections.Generic;

namespace Perlang.Interpreter
{
    /// <summary>
    /// Callable implementation for user-defined functions.
    /// </summary>
    internal class PerlangFunction : ICallable
    {
        private readonly Stmt.Function declaration;
        private readonly IEnvironment closure;

        internal PerlangFunction(Stmt.Function declaration, IEnvironment closure)
        {
            this.declaration = declaration;
            this.closure = closure;
        }

        public object Call(IInterpreter interpreter, List<object> arguments)
        {
            var environment = new PerlangEnvironment(closure);

            for (int i = 0; i < declaration.Params.Count; i++)
            {
                environment.Define(declaration.Params[i].Lexeme, expr: null, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(declaration.Body, environment);
                return null;
            }
            catch (Return returnValue)
            {
                return returnValue.Value;
            }
        }

        public int Arity()
        {
            return declaration.Params.Count;
        }

        public override string ToString()
        {
            return "<fn " + declaration.Name.Lexeme + ">";
        }
    }
}
