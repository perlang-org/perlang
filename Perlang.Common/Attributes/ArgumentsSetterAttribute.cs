using System;
using JetBrains.Annotations;

namespace Perlang.Attributes
{
    /// <summary>
    /// Annotates a .NET class for receiving command line arguments when a Perlang program starts.
    ///
    /// The attribute is intended to be placed on  a method with the following signature:
    ///
    /// <code>
    /// public static void SetArguments(ImmutableList&lt;string&gt; arguments)
    /// </code>
    ///
    /// Note that the method might be called multiple times, if multiple Perlang programs are being started during the
    /// lifetime of the process. This is typically what happens when the Perlang unit tests are executed.
    /// </summary>
    [MeansImplicitUse]
    public class ArgumentsSetterAttribute : Attribute
    {
    }
}
