using System;

namespace Perlang;

[Flags]
public enum FunctionModifiers
{
    None = 0,
    Static = 1,
    Extern = 2,
    Implement = 4
}
