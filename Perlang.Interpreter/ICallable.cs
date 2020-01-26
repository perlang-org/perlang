using System.Collections.Generic;

namespace Perlang.Interpreter
{
    internal interface ICallable
    {
        object Call(IInterpreter interpreter, List<object> arguments);
        int Arity();
    }
}