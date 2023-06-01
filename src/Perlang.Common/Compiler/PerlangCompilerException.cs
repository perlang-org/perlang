using System;

namespace Perlang.Compiler;

public class PerlangCompilerException : Exception
{
    public PerlangCompilerException(string message)
        : base(message)
    {
    }
}
