using System.Collections.Generic;

namespace Perlang
{
    /// <summary>
    /// Interface for callable functions, both user-defined and parts of stdlib.
    /// </summary>
    public interface ICallable
    {
        object Call(IInterpreter interpreter, List<object> arguments);
        int Arity();
        bool VariadicArguments();
    }
}
