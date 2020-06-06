using System;
using System.Collections.Generic;
using System.Text;

namespace Perlang.Stdlib.Callables
{
    [GlobalCallable("base64_encode")]
    public class Base64EncodeCallable
    {
        public object Call(IInterpreter interpreter, string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            return Convert.ToBase64String(plainTextBytes, Base64FormattingOptions.InsertLineBreaks);
        }
    }
}
