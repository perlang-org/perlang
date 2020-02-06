using System;

namespace Perlang
{
    /// <summary>
    /// Annotates an <see cref="ICallable"/> for automatic registration in the global namespace.
    /// </summary>
    public class GlobalCallableAttribute : Attribute
    {
        public string Name { get; }

        public GlobalCallableAttribute(string name)
        {
            Name = name;
        }
    }
}
