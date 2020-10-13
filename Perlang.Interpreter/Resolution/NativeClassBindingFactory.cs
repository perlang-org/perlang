using System;

namespace Perlang.Interpreter.Resolution
{
    internal class NativeClassBindingFactory : IBindingFactory
    {
        private readonly Type type;

        public NativeClassBindingFactory(Type type)
        {
            this.type = type ?? throw new ArgumentException("type cannot be null");
        }

        public Binding CreateBinding(int distance, Expr referringExpr)
        {
            return new NativeClassBinding(referringExpr, type);
        }
    }
}
