using CppSharp;

namespace Perlang.GenerateCppSharpBindings;

public static class Program
{
    public static void Main()
    {
        ConsoleDriver.Run(new PerlangCliLibrary());
    }
}
