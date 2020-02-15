using System;
using System.Collections.Generic;
using System.Text;

namespace Perlang.Stdlib.Callables
{
    [GlobalCallable("base64_encode")]
    public class Base64EncodeCallable : ICallable
    {
        public object Call(IInterpreter interpreter, List<object> arguments)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes((string)arguments[0]);

            return Convert.ToBase64String(plainTextBytes, Base64FormattingOptions.InsertLineBreaks);
        }

        public int Arity()
        {
            return 1;
        }
    }
}
