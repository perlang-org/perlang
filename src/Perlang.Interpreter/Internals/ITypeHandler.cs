#nullable enable

namespace Perlang.Interpreter.Internals;

public interface ITypeHandler
{
    void AddType(string nameLexeme, IPerlangType perlangInterface);
    void AddEnum(string nameLexeme, PerlangEnum perlangEnum);

    /// <summary>
    /// Returns a user-defined type with the given name.
    /// </summary>
    /// <param name="name">The name of the type, e.g. Greeter or IGreeter.</param>
    /// <returns>An <see cref="IPerlangType"/> reference with data about the type, or <c>null</c> if no matching type
    /// could be found.</returns>
    IPerlangType? GetType(string name);
}
