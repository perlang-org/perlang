using System.Collections.Generic;

namespace Perlang
{
    public interface IInterpreter
    {
        List<string> Arguments { get; }

        void ExecuteBlock(IEnumerable<Stmt> statements, IEnvironment blockEnvironment);
        void Resolve(Expr expr, int depth);
    }
}
