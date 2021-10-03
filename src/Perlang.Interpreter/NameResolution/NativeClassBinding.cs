using System;

namespace Perlang.Interpreter.NameResolution
{
    /// <summary>
    /// Binding implementation for referring to native .NET classes.
    /// </summary>
    internal class NativeClassBinding : Binding
    {
        public Type Type { get; }

        // The fact that this is a "native" class is an implementation detail. The end goal is that Perlang classes and
        // "native" classes should be indistinguishable; in fact, Perlang classes aim to become full CLR classes in the
        // long run.
        public override string ObjectType => "class";

        internal NativeClassBinding(Expr referringExpr, Type type)
            : base(new TypeReference(type), referringExpr)
        {
            Type = type;
        }
    }
}
