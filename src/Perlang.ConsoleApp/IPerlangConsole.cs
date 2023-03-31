using System.CommandLine;

namespace Perlang.ConsoleApp;

public interface IPerlangConsole : IConsole
{
    void WriteStdoutLine(Lang.String s);
}
