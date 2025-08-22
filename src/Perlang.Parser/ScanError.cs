using System;

namespace Perlang.Parser;

public class ScanError : Exception
{
    public string FileName { get; }
    public int Line { get; }

    public ScanError(string message, string fileName, int line)
        : base(message)
    {
        FileName = fileName;
        Line = line;
    }
}