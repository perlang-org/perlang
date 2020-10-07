using System.Collections.Generic;
using Perlang.Interpreter.Resolution;
using Perlang.Interpreter.Typing;
using Perlang.Parser;

namespace Perlang.Tests.Integration
{
    internal class EvalResult
    {
        public object Value { get; set; }
        public List<ParseError> ParseErrors { get; } = new List<ParseError>();
        public List<RuntimeError> RuntimeErrors { get; } = new List<RuntimeError>();
        public List<ResolveError> ResolveErrors { get; } = new List<ResolveError>();
        public TypeValidationErrors TypeValidationErrors { get; } = new TypeValidationErrors();
    }
}
