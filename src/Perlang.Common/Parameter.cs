namespace Perlang
{
    public class Parameter
    {
        public Token Name { get; }
        public ITypeReference TypeReference { get; }

        public Token TypeSpecifier => TypeReference.TypeSpecifier;

        public Parameter(Token name, ITypeReference typeReference)
        {
            Name = name;
            TypeReference = typeReference;
        }
    }
}
