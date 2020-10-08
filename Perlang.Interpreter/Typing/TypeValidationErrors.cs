using System.Collections.Generic;

namespace Perlang.Interpreter.Typing
{
    public class TypeValidationErrors : List<TypeValidationError>
    {
        public bool Empty() => Count == 0;
    }
}
