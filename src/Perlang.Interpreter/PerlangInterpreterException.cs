using System;

namespace Perlang.Interpreter;

public class PerlangInterpreterException : Exception
{
    public PerlangInterpreterException(string message)
        : base(message)
    {
    }
}