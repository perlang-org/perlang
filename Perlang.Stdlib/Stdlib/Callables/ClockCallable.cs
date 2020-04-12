using System;
using System.Collections.Generic;

namespace Perlang.Stdlib.Callables
{
    [GlobalCallable("clock")]
    public class Clock
    {
        public object Call(IInterpreter interpreter)
        {
            return new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds() / 1000.0;
        }

        public override string ToString()
        {
            return "<native fn>";
        }
    }
}
