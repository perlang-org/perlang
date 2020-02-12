using System.Collections.Generic;
using Perlang.Exceptions;

namespace Perlang.Stdlib.Callables
{
    [GlobalCallable("argv_pop")]
    public class ArgvPopCallable :  ICallable
    {
        public object Call(IInterpreter interpreter, List<object> arguments)
        {
            if (interpreter.Arguments.Count < 1)
            {
                throw new IllegalStateException("No arguments left");
            }

            string argument = interpreter.Arguments[0];
            interpreter.Arguments.RemoveAt(0);

            return argument;
        }

        public int Arity()
        {
            return 0;
        }
    }
}
