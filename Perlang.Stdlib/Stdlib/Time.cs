using System;
using Perlang.Attributes;

namespace Perlang.Stdlib
{
    [GlobalClass]
    public static class Time
    {
        public static DateTime Now() =>
            DateTime.Now;
    }
}
