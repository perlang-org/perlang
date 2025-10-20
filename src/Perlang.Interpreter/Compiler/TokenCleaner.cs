#nullable enable
using System;
using System.Collections.Generic;

namespace Perlang.Interpreter.Compiler;

internal static class TokenCleaner
{
    public static void DisposeOnShutdown(IEnumerable<IToken> tokens)
    {
        AppDomain.CurrentDomain.ProcessExit += (sender, args) => {
            foreach (IToken token in tokens) {
                if (token is Token t) {
                    // t.Dispose() wouldn't be enough since __ownsNativeInstance is false for the instances returned by
                    // perlang_cli.CreateNullToken() etc, because CppSharp doesn't know that ownership for this class could
                    // be handled on the C# side. We work around this by calling a delete method manually here.
                    perlang_cli.DeleteToken(t);
                }
            }
        };
    }
}
