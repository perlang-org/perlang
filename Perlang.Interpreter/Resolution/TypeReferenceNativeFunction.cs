using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace Perlang.Interpreter.Resolution
{
    /// <summary>
    /// Container class for TypeReference information for a native .NET function.
    ///
    /// The native object is expected to have a Call method which compiles with the following signature:
    ///
    /// T Call(IInterpreter interpreter, A1 argument1, A2 argument2...)
    ///
    /// The type of T is determined by the TypeReference.ClrType property. The types of A1, A2... are determined by
    /// the ParameterTypes property.
    /// </summary>
    internal class TypeReferenceNativeFunction
    {
        public TypeReference ReturnTypeReference { get; }
        public object Callable { get; }
        public MethodInfo Method { get; }
        public ImmutableArray<Type> ParameterTypes { get; }

        public TypeReferenceNativeFunction(TypeReference returnTypeReference, object callable, MethodInfo method, IEnumerable<Type> parameterTypes)
        {
            ReturnTypeReference = returnTypeReference;
            Callable = callable;
            Method = method;
            ParameterTypes = parameterTypes.ToImmutableArray();
        }
    }
}
