using System.Collections.Immutable;

namespace Perlang.Interpreter.Resolution
{
    internal interface INamedParameterizedBinding
    {
        public string FunctionName { get; }
        public ImmutableArray<Parameter> Parameters { get; }
    }
}
