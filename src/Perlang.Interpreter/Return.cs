using System;
using System.Diagnostics.CodeAnalysis;

namespace Perlang.Interpreter;

[SuppressMessage("SonarAnalyzer.CSharp", "S3871", Justification = "Exception is not propagated outside assembly")]
internal class Return : Exception
{
    internal object Value { get; }

    internal Return(object value)
    {
        Value = value;
    }
}