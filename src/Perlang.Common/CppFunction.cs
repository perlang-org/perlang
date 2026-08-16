using System.Collections.Immutable;

namespace Perlang;

public class CppFunction : IPerlangFunction
{
    public string Name { get; }
    public ImmutableList<Parameter> Parameters { get; }
    public ITypeReference ReturnTypeReference { get; }

    // TODO: This is not correct and will likely have to be fixed at some point
    public FunctionModifiers FunctionModifiers => FunctionModifiers.None;

    public CppFunction(string name, ImmutableList<Parameter> parameters, ITypeReference returnTypeReference)
    {
        Name = name;
        Parameters = parameters;

        // Note: the CppType property of this type reference can be null at this point, since type resolving might not
        // have taken place at this point.
        ReturnTypeReference = returnTypeReference;
    }
}
