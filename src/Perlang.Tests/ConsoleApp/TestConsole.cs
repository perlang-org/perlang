using System;
using System.CommandLine.IO;
using System.Text;
using Perlang.ConsoleApp;

namespace Perlang.Tests.ConsoleApp;

// Heavily inspired by System.CommandLine.IO.TestConsole (MIT-licensed)
public class TestConsole : IPerlangConsole
{
    private readonly StringBuilder stringBuilder;

    public IStandardStreamWriter Out { get; }
    public bool IsOutputRedirected => false;

    public IStandardStreamWriter Error { get; }
    public bool IsErrorRedirected => false;

    public bool IsInputRedirected => false;

    public TestConsole()
    {
        stringBuilder = new StringBuilder();
        Out = new StandardStreamWriter(stringBuilder);
        Error = new StandardStreamWriter(new StringBuilder());
    }

    public void WriteStdoutLine(Perlang.Lang.String s)
    {
        stringBuilder.Append(s.ToString());
        stringBuilder.Append(Environment.NewLine);
    }

    private class StandardStreamWriter : IStandardStreamWriter
    {
        private readonly StringBuilder stringBuilder;

        public StandardStreamWriter(StringBuilder stringBuilder)
        {
            this.stringBuilder = stringBuilder;
        }

        public void Write(string? value)
        {
            stringBuilder.Append(value);
        }

        public override string ToString()
        {
            return stringBuilder.ToString();
        }
    }
}
