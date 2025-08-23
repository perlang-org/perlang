namespace Perlang;

/// <summary>
/// C++ methods which acts as "property getters", as in C#.
/// </summary>
public class CppPropertyGetter : IPerlangField
{
    public string Name { get; }
    public ITypeReference TypeReference { get; }
    public string MethodName { get; }

    public CppPropertyGetter(string name, ITypeReference typeReference, string methodName)
    {
        Name = name;
        TypeReference = typeReference;
        MethodName = methodName;
    }
}
