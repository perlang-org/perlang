using System.Linq;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Immutability
{
    public class SuperGlobals
    {
        [Fact]
        public void ARGV_cannot_be_overwritten_by_assignment()
        {
            string source = @"
                ARGV = 42
            ";

            var result = EvalWithValidationErrorCatch(source);
            var exception = result.Errors.First();

            Assert.Single(result.Errors);

            Assert.Matches("Object 'ARGV' is immutable and cannot be modified.", exception.Message);
        }
    }
}
