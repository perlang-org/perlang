using System.Collections.Generic;

namespace Perlang
{
    /// <summary>
    /// Interface for user-defined functions.
    ///
    /// Note that this interface is not used to call "native" (.NET-defined) functions. For those, we use reflection
    /// to be able to support methods with multiple parameters in a more convenient manner.
    /// </summary>
    public interface ICallable
    {
        object Call(IInterpreter interpreter, List<object> arguments);
        int Arity();
    }
}
