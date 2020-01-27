using Xunit.Sdk;

namespace Perlang.Tests
{
    /// <summary>
    /// Utility assert methods to complement xUnit.net
    /// </summary>
    internal static class MoreAssert
    {
        public static void Fail(string message)
        {
            throw new XunitException(message);
        }
    }
}