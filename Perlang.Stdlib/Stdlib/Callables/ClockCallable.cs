using System;
using System.Collections.Generic;

namespace Perlang.Stdlib.Callables
{
    [GlobalCallable("clock")]
    public class Clock : ICallable
    {
        public int Arity()
        {
            return 0;
        }

        public object Call(IInterpreter interpreter, List<object> arguments)
        {
            return new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds() / 1000.0;
        }

        public override string ToString()
        {
            return "<native fn>";
        }
    }
}
