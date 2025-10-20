namespace Perlang;

public class Parameter
{
    public IToken Name { get; }
    public ITypeReference TypeReference { get; }

    public IToken TypeSpecifier => TypeReference.TypeSpecifier;
    public bool IsArray => TypeReference.IsArray;

    public Parameter(IToken name, ITypeReference typeReference)
    {
        Name = name;
        TypeReference = typeReference;
    }
}
