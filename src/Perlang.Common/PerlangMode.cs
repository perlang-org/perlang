using System;
using System.IO;

namespace Perlang;

public static class PerlangMode
{
    public static bool ExperimentalCompilation
    {
        get
        {
            string environmentVariable = Environment.GetEnvironmentVariable("PERLANG_EXPERIMENTAL_COMPILATION");

            // The environment variable takes precedence, if set
            if (Boolean.TryParse(environmentVariable, out bool flag))
            {
                return flag;
            }

            string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            return File.Exists(Path.Combine(homeDirectory, ".perlang_experimental_compilation"));
        }
    }
}
