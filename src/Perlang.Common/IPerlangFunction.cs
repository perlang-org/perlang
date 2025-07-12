using System.Collections.Immutable;

namespace Perlang
{
    /// <summary>
    /// Interface for Perlang function implementations.
    /// </summary>
    public interface IPerlangFunction
    {
        public string Name { get; }
        public ImmutableList<Parameter> Parameters { get; }
        public ITypeReference ReturnTypeReference { get; }
    }

    public class CppFunction : IPerlangFunction
    {
        public string Name { get; }
        public ImmutableList<Parameter> Parameters { get; }
        public ITypeReference ReturnTypeReference { get; }

        public CppFunction(string name, ImmutableList<Parameter> parameters, ITypeReference returnTypeReference)
        {
            Name = name;
            Parameters = parameters;
            ReturnTypeReference = returnTypeReference;
        }
    }
}
