using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Perlang.Interpreter.Resolution
{
    /// <summary>
    /// A binding to a native (.NET) method.
    ///
    /// "Native" in this sense means that the method is native to the runtime in which it is being executed. It should
    /// not be confused with machine-native, unmanaged code (i.e. code written in languages like C, C++, Rust or Go.)
    /// </summary>
    internal class NativeBinding : Binding
    {
        public MethodInfo Method { get; }
        public string FunctionName { get; }
        public ImmutableArray<Parameter> Parameters { get; }

        public NativeBinding(MethodInfo method, string functionName, IEnumerable<Type> parameterTypes, TypeReference returnTypeReference, Expr referringExpr)
            : base(returnTypeReference, referringExpr)
        {
            Method = method;
            FunctionName = functionName;

            Parameters = parameterTypes
                .Select(t => new Parameter(new TypeReference(t)))
                .ToImmutableArray();
        }
    }
}
