using System.Collections.Generic;

namespace Perlang.Interpreter
{
    class PerlangFunction : ICallable
    {
        private readonly Stmt.Function declaration;
        private readonly PerlangEnvironment closure;

        internal PerlangFunction(Stmt.Function declaration, PerlangEnvironment closure)
        {
            this.declaration = declaration;
            this.closure = closure;
        }

        public object Call(IInterpreter interpreter, List<object> arguments)
        {
            var environment = new PerlangEnvironment(closure);

            for (int i = 0; i < declaration.Params.Count; i++)
            {
                environment.Define(declaration.Params[i].Lexeme, arguments[i]);
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