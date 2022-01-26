using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Perlang.Tests.Integration
{
    public class TestCultures : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            // A culture which uses 123.45 number format. Resembles the C locale in POSIX systems, which is the default
            // unless a locale is explicitly specified. (See https://man7.org/linux/man-pages/man7/locale.7.html for
            // more details)
            yield return new object[] { CultureInfo.GetCultureInfo("en-US") };

            // A culture which uses 123,45 number format.
            yield return new object[] { CultureInfo.GetCultureInfo("sv-SE") };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
