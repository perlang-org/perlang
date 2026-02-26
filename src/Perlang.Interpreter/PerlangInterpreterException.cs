using System;

namespace Perlang.Interpreter;

public class PerlangInterpreterException : Exception
{
    // TODO: Consider replacing with PerlangCompilerException everywhere, since we are no longer an interpreter
    public PerlangInterpreterException(string message)
        : base(message)
    {
    }
}
