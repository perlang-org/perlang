using System.Collections.Generic;
using Perlang.Parser;

namespace Perlang.Interpreter
{
    public class ResolveError
    {
        public Token Token { get; set; }
        public string Message { get; set; }
    }

    public class ResolveErrors : List<ResolveError>
    {
        public bool Empty() => Count == 0;

        // Convenience method to free consumers from having to construct ScanErrors manually.
        public void Add(Token token, string message)
        {
            Add(new ResolveError { Token = token, Message = message });
        }
    }
}