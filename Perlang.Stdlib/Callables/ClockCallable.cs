using System;
using System.Collections.Generic;

namespace Perlang.Stdlib.Callables
{
    [GlobalCallable("clock")]
    public class Clock : NativeCallable
    {
        public override int Arity()
        {
            return 0;
        }

        public override object Call(IInterpreter interpreter, List<object> arguments)
        {
            return new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds() / 1000.0;
        }
    }
}
