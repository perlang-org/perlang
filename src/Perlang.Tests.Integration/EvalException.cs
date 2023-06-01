#nullable enable
using System;

namespace Perlang.Tests.Integration;

public class EvalException : Exception
{
    public EvalException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
