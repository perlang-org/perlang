using System.Linq;
using System.Reflection;

namespace Perlang
{
    /// <summary>
    /// Contains common constants shared between multiple assemblies.
    ///
    /// This mechanism serves two purposes:
    ///
    /// - To avoid duplication of the version identifier among multiple assemblies (which would make it unnecessarily
    ///   awkward to bump version numbers).
    /// - To make it possible to retrieve the version identifier(s) programmatically, at runtime via reflection.
    /// </summary>
    public static partial class CommonConstants
    {
        public const string Version = GitTagVersion;
        public const string InformationalVersion = GitDescribeVersion;

        public static string GetFullVersion() =>
            ((AssemblyInformationalVersionAttribute)Assembly
                .GetAssembly(typeof(CommonConstants))
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), inherit: false)
                .First())
            .InformationalVersion;
    }
}
