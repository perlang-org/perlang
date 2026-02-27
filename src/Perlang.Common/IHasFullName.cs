namespace Perlang;

public interface IHasFullName
{
    string[] FullNameParts { get; }
    string FullName { get; }
}
