using System.Collections.Immutable;

namespace Perlang;

/// <summary>
/// Interface for Perlang function implementations.
/// </summary>
public interface IPerlangFunction
{
    public string Name { get; }
    public ImmutableList<Parameter> Parameters { get; }
    public ITypeReference ReturnTypeReference { get; }
}
