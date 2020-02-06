using System.Collections.Generic;

namespace Perlang
{
    public interface ICallable
    {
        object Call(IInterpreter interpreter, List<object> arguments);
        int Arity();
    }
}
