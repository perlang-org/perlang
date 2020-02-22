using System;
using System.Collections.Generic;
using System.Text;

namespace Perlang.Stdlib.Callables
{
    [GlobalCallable("base64_decode")]
    public class Base64DecodeCallable : ICallable
    {
        public object Call(IInterpreter interpreter, List<object> arguments)
        {
            var base64Data = (string)arguments[0];

            var data = Convert.FromBase64String(base64Data);

            // For now, blindly assume that the base64-encoded data is a valid UTF-8/ASCII string and parse it as such.
            // Future improvements here could be to make it possible to return a byte array instead, but for now,
            // returning a string is a much morre practically useful approach.
            return Encoding.UTF8.GetString(data);
        }

        public int Arity()
        {
            return 1;
        }
    }
}
