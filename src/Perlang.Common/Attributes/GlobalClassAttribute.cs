using System;
using JetBrains.Annotations;

namespace Perlang.Attributes
{
    /// <summary>
    /// Annotates a class for being registered in the global, top-level Perlang namespace.
    ///
    /// Think of this as something like the "System" namespace in .NET. At the moment, the global namespace really
    /// doesn't have a namespace prefix. This might change at some point if we find compelling reasons for this,
    /// but making global things really be global is the simplest approach for now.
    ///
    /// For the time being, decorating a class with this attribute means that all public methods become callable from
    /// Perlang code. For static methods, they are called with `Class.method_name()` notation; if the class
    /// is non-static, it can be instantiated from Perlang just like any other (Perlang) class and instance methods can
    /// be called using `instance.method_name()` notation. Perlang respects the visibility of methods - if
    /// they are private, Perlang will produce a compile-time error if an attempt is made to call the method.
    /// </summary>
    [MeansImplicitUse(ImplicitUseTargetFlags.Members)]
    public class GlobalClassAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets an optional custom name for the class. If not provided, the short-name of the class will be used.
        /// </summary>
        public string Name { get; set; }
    }
}
