using System;

namespace Perlang.Interpreter
{
    internal class Return : Exception
    {
        internal object Value { get; }

        internal Return(object value)
        {
            Value = value;
        }
    }
}