namespace Perlang;

public interface IPerlangField
{
    public string Name { get; }
    public ITypeReference TypeReference { get; }
}
