#nullable enable
using Perlang.Interpreter.NameResolution;

namespace Perlang.Interpreter.Internals;

public interface IBindingRetriever
{
    Binding? GetVariableOrFunctionBinding(Expr referringExpr);
}
