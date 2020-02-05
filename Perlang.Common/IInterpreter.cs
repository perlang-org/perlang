using System.Collections.Generic;

namespace Perlang
{
    public interface IInterpreter
    {
        void ExecuteBlock(IEnumerable<Stmt> statements, IEnvironment blockEnvironment);
        void Resolve(Expr expr, int depth);
    }
}
