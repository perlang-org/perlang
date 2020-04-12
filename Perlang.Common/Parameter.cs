namespace Perlang
{
    public class Parameter
    {
        public Token Name { get; }
        public TypeReference TypeReference { get; }

        public Token TypeSpecifier => TypeReference.TypeSpecifier;

        public Parameter(TypeReference typeReference)
        {
            TypeReference = typeReference;
        }

        public Parameter(Token name, TypeReference typeReference)
        {
            Name = name;
            TypeReference = typeReference;
        }
    }
}
