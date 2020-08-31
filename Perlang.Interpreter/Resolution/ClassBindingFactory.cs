using System;

namespace Perlang.Interpreter.Resolution
{
    internal class ClassBindingFactory : IBindingFactory
    {
        private PerlangClass PerlangClass { get; }

        public ClassBindingFactory(PerlangClass perlangClass)
        {
            PerlangClass = perlangClass ?? throw new ArgumentException("clrType cannot be null");
        }

        public Binding CreateBinding(int distance, Expr referringExpr)
        {
            return new ClassBinding(null, referringExpr, PerlangClass);
        }
    }
}
