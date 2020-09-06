using System.Reflection;
using System.Runtime.Loader;

namespace Perlang.Interpreter
{
    public class PerlangAssemblyLoadContext : AssemblyLoadContext
    {
        public PerlangAssemblyLoadContext() :
            base(isCollectible: true)
        {
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            // Returning null here means means that all the dependency assemblies are loaded into the default context,
            // and the new context contains only the assemblies explicitly loaded into it.
            return null;
        }
    }
}
