using System;
using System.Collections.Generic;

namespace Perlang
{
    public interface IInterpreter
    {
        void ExecuteBlock(IEnumerable<Stmt> statements, IEnvironment blockEnvironment);
        void Resolve(Expr expr, int depth);

        /// <summary>
        /// A callback which will receive all output written to the standard output from this interpreter instance.
        /// </summary>
        Action<string> StandardOutputHandler { get; }
    }
}
