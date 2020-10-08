using System;

namespace Perlang.Interpreter.Resolution
{
    internal class NativeClassBinding : Binding
    {
        public Type Type { get; }

        internal NativeClassBinding(Expr referringExpr, Type type)
            : base(new TypeReference(type), referringExpr)
        {
            Type = type;
        }
    }
}
