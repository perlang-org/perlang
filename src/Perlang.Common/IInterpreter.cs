using System.Collections.Generic;

namespace Perlang
{
    /// <summary>
    /// Shared interface for the Perlang interpreter instance.
    /// </summary>
    public interface IInterpreter
    {
        /// <summary>
        /// Executes the given block (list of statements) in the given environment.
        /// </summary>
        /// <param name="statements">The statements for the block that should be executed.</param>
        /// <param name="blockEnvironment">The environment in which it should be executed.</param>
        void ExecuteBlock(IEnumerable<Stmt> statements, IEnvironment blockEnvironment);
    }
}
