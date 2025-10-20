#nullable enable
using System;

namespace Perlang;

// The naming of this might be hard to justify. jlox used RuntimeError, and we just followed along in its path in
// this case. I think we'll live with it for now; hopefully we can make Perlang a "dynamically compiled" language at
// some point anyway, at which point we should be able to completely get rid of this class altogether.
public class RuntimeError : Exception
{
    public IToken? Token { get; }

    public RuntimeError(IToken? token, string message)
        : base(message)
    {
        Token = token;
    }
}
