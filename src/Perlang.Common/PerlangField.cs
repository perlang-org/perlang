namespace Perlang;

public class PerlangField : IPerlangField
{
    public string Name { get; }
    public ITypeReference TypeReference { get; }

    public PerlangField(string name, ITypeReference typeReference)
    {
        TypeReference = typeReference;
        Name = name;
    }
}
