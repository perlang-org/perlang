using System.Collections.Generic;

namespace Perlang.Interpreter
{
    public interface IInterpreter
    {
        void ExecuteBlock(IEnumerable<Stmt> statements, PerlangEnvironment blockEnvironment);
        void Resolve(Expr expr, int depth);
    }
}