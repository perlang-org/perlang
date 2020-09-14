using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Perlang.Interpreter.Resolution
{
    /// <summary>
    /// A binding to a .NET <see cref="MethodInfo"/> object.
    ///
    /// The method being referred to here can be either a native .NET method, or (in the future when we support) a
    /// native .NET method being defined from Perlang.
    /// </summary>
    internal class MethodBinding : Binding, INamedParameterizedBinding
    {
        public MethodInfo Method { get; }
        public string FunctionName { get; }
        public ImmutableArray<Parameter> Parameters { get; }

        public MethodBinding(MethodInfo method, Expr referringExpr) :
            base(null, referringExpr)
        {
            Method = method;
            FunctionName = method.Name;

            Parameters = method.GetParameters()
                .Select(p => new Parameter(new TypeReference(p.ParameterType)))
                .ToImmutableArray();
        }
    }
}
