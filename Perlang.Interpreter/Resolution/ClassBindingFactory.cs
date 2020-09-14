using System;

namespace Perlang.Interpreter.Resolution
{
    internal class ClassBindingFactory : IBindingFactory
    {
        private readonly PerlangClass perlangClass;

        public ClassBindingFactory(PerlangClass perlangClass)
        {
            this.perlangClass = perlangClass ?? throw new ArgumentException("perlangClass cannot be null");
        }

        public Binding CreateBinding(int distance, Expr referringExpr)
        {
            return new ClassBinding(referringExpr, perlangClass);
        }
    }
}
