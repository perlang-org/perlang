using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Arrays;

public class ClassArrayTests
{
    [Fact]
    public void array_of_user_defined_class_can_be_defined()
    {
        string source = """
            public class Name {
                private name_: string;

                public constructor(name: string)
                {
                    name_ = name;
                }

                public name(): string
                {
                    return name_;
                }
            }

            var a: Name[] = [new Name("Alice"), new Name("Bob"), new Name("Charlie")];

            print a[0].name();
            print a[1].name();
            print a[2].name();
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .BeEquivalentTo(
                "Alice",
                "Bob",
                "Charlie"
            );
    }
}
