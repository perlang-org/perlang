#nullable enable
using System.Collections.Immutable;

namespace Perlang;

public interface IPerlangType
{
    public string Name { get; }
    public ImmutableList<IPerlangFunction> Methods { get; }
    public ImmutableList<IPerlangField> Fields { get; }
}
