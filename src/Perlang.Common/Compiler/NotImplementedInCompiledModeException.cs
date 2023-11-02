namespace Perlang.Compiler;

/// <summary>
/// Exception thrown for valid Perlang code, which is currently "not yet supported" in compiled mode.
///
/// This exception is thrown to make it possible for integration tests to skip tests for code which is known to not
/// yet work.
/// </summary>
public class NotImplementedInCompiledModeException : PerlangCompilerException
{
    public NotImplementedInCompiledModeException(string message)
        : base(message)
    {
    }
}
