using System.Linq;
using System.Reflection;

namespace Perlang
{
    public static partial class CommonConstants
    {
        public const string Version = "0.1.0";

        public static string GetFullVersion() =>
            ((AssemblyInformationalVersionAttribute)Assembly
                .GetAssembly(typeof(CommonConstants))
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), inherit: false)
                .First())
            .InformationalVersion;
    }
}
