using System;
using System.IO;

namespace Perlang;

public static class PerlangMode
{
    public static bool ExperimentalCompilation => !ExperimentalInterpretation;

    private static bool ExperimentalInterpretation
    {
        get
        {
            // This used to be an environment variable PERLANG_EXPERIMENTAL_COMPILATION. Now that compilation is enabled
            // by default, we have flipped the flag to instead be PERLANG_EXPERIMENTAL_INTERPRETATION. The interpreter
            // part (which will in fact emit native code via an LLVM backend) is not yet implemented, but we're keeping
            // the infrastructure in place to make room for it until if/when it gets implemented.
            string environmentVariable = Environment.GetEnvironmentVariable("PERLANG_EXPERIMENTAL_INTERPRETATION");

            // The environment variable takes precedence, if set
            if (Boolean.TryParse(environmentVariable, out bool flag))
            {
                return flag;
            }

            string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            return File.Exists(Path.Combine(homeDirectory, ".perlang_experimental_interpretation"));
        }
    }
}
