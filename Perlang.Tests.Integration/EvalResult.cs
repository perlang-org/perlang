#nullable enable
using System.Collections.Generic;
using Perlang.Interpreter;
using Perlang.Interpreter.Resolution;
using Perlang.Parser;

namespace Perlang.Tests.Integration
{
    // TODO: Change to be a generic class, with ParseError, RuntimeError etc as the type parameter.
    internal class EvalResult
    {
        public object? Value { get; set; }
        public List<ParseError> ParseErrors { get; } = new List<ParseError>();
        public List<RuntimeError> RuntimeErrors { get; } = new List<RuntimeError>();
        public List<ResolveError> ResolveErrors { get; } = new List<ResolveError>();
        public List<ValidationError> ValidationErrors { get; } = new List<ValidationError>();
    }
}
