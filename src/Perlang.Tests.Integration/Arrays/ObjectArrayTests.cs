using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Arrays;

public class ObjectArrayTests
{
    // "Object" in this class can be read as "instance of user-defined class"

    [Fact]
    public void object_array_can_be_defined()
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

    [Fact]
    public void explicitly_typed_object_array_with_fixed_size_can_be_indexed()
    {
        string source = """
            public class CustomType
            {
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

            var a: CustomType[] = new CustomType[3];

            a[0] = new CustomType("one");
            a[1] = new CustomType("two");
            a[2] = new CustomType("three");

            print a[0].name();
            print a[1].name();
            print a[2].name();
            """;

        var output = EvalReturningOutput(source);

        output.Should()
            .BeEquivalentTo(
                "one",
                "two",
                "three"
            );
    }

    [Fact]
    public void indexing_object_array_with_negative_index_produces_expected_runtime_error()
    {
        string source = """
            public class SomeClass
            {
                public name(): string
                {
                    return "SomeClass";
                }
            }

            var a: SomeClass[] = [new SomeClass(), new SomeClass(), new SomeClass()];

            print a[-1].name();
            """;

        var result = EvalWithRuntimeErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("exited with exit code 134");

        result.OutputAsString.Should()
            .Contain("index out of range (18446744073709551615 > 2)");
    }

    [Fact]
    public void indexing_object_array_with_initializer_outside_of_boundaries_produces_expected_runtime_error()
    {
        string source = """
            public class SomeClass
            {
                public name(): string
                {
                    return "SomeClass";
                }
            }

            var a: SomeClass[] = [new SomeClass(), new SomeClass(), new SomeClass()];

            // a[2] is the last element of the array
            print a[3].name();
            """;

        var result = EvalWithRuntimeErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("exited with exit code 134");

        result.OutputAsString.Should()
            .Contain("index out of range (3 > 2)");
    }

    [Fact]
    public void indexing_object_array_with_fixed_size_outside_of_boundaries_produces_expected_runtime_error()
    {
        string source = """
            public class SomeClass
            {
                public name(): string
                {
                    return "SomeClass";
                }
            }

            var a: SomeClass[] = new SomeClass[3];

            // a[2] is the last element of the array
            print a[3].name();
            """;

        var result = EvalWithRuntimeErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("exited with exit code 134");

        result.OutputAsString.Should()
            .Contain("index out of range (3 > 2)");
    }

    [Fact]
    public void indexing_uninitialized_object_array_with_fixed_size_produces_expected_runtime_error()
    {
        string source = """
            public class SomeClass
            {
                public name(): string
                {
                    return "SomeClass";
                }
            }

            var a: SomeClass[];

            print a[0].name();
            """;

        var result = EvalWithRuntimeErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("exited with exit code 134");

        // Ideally, this would be detected by the compiler, but the problem is that the 'var a: int[]' part above gets
        // translated to 'std::shared_ptr<perlang::IntArray> a'. In other words, a local stack-allocated std::shared_ptr
        // which is initialized with a null pointer. Regretfully, this is not an "uninitialized" variable per se but a
        // variable initialized to null, so we don't get any help by the C++ compiler in this case.
        result.OutputAsString.Should()
            .Contain("terminate called after throwing an instance of 'perlang::NullPointerException");
    }
}
