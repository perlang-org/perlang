using System;
using Perlang.Attributes;

namespace Perlang.Stdlib.Callables
{
    [GlobalFunction("clock")]
    public class ClockCallable
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
