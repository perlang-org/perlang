using System.Collections.Generic;

namespace Perlang
{
    /// <summary>
    /// Base class for callables implemented as "native" C#/.NET code.
    /// </summary>
    public abstract class NativeCallable : ICallable
    {
        public abstract object Call(IInterpreter interpreter, List<object> arguments);
        public abstract int Arity();

        public bool VariadicArguments()
        {
            return Arity() == -1;
        }

        public override string ToString()
        {
            return "<native fn>";
        }
    }
}
