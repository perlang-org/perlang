using System;

namespace Perlang.Attributes
{
    /// <summary>
    /// Annotates a native .NET method for automatic registration in the global namespace.
    ///
    /// The attribute can only be applied to static methods, since there is no relevant "this" object in this context.
    ///
    /// See also <seealso cref="GlobalClassAttribute"/>.
    /// </summary>
    public class GlobalFunctionAttribute : Attribute
    {
        public string Name { get; }

        public GlobalFunctionAttribute(string name)
        {
            Name = name;
        }
    }
}
