using FluentAssertions;
using Xunit;
using static Perlang.Tests.Integration.EvalHelper;

namespace Perlang.Tests.Integration.Interface;

public class InterfaceTests
{
    [Fact]
    public void interface_method_can_be_implemented_and_called_using_interface_type()
    {
        string source = """
            public interface IGreeter
            {
                public say_hello(): void;
            }

            public class Greeter : IGreeter
            {
                // Interface methods must be marked with 'implement'. This makes it impossible to implement an interface
                // by "accident", if e.g. a new method gets added to an interface controlled by a 3rd party.
                public implement say_hello(): void
                {
                    print("Hello World from interface implementing method");
                }
            }

            var greeter: IGreeter = new Greeter();
            greeter.say_hello();
            """;

        var output = EvalReturningOutputString(source);

        output.Should()
            .Be("Hello World from interface implementing method");
    }

    [Fact]
    public void interface_cannot_be_inherited()
    {
        string source = """
            public interface IService
            {
                public name(): void;
            }

            // This is the currently unsupported part (interfaces cannot implement other interfaces)
            public interface IHttpService : IService
            {
            }

            public class App : IHttpService
            {
                public implement name(): string
                {
                    return "App";
                }
            }

            // Deliberately typed as IHttpService, to ensure that methods in a parent interface can be called using an
            // interface type too
            var service: IHttpService = new App();
            print service.name();
            """;

        var result = EvalWithParseErrorCatch(source);

        // Cannot use ContainSingle() here, since we get a parse error on a later line as well. Haven't figured out why,
        // but it's likely the parser gets into a messed up state because of the first error
        result.Errors.Should()
            .Contain(e => e.Message.Contains("Expecting '{' before interface body"));
    }

    [Fact]
    public void non_interface_method_cannot_be_called_using_interface_type()
    {
        string source = """
            public interface IFrobnicator
            {
                public frobnicate(): void;
            }

            public class Frobnicator : IFrobnicator
            {
                public implement frobnicate(): void
                {
                }

                public do_something_else(): void
                {
                }
            }

            var frobnicator: IFrobnicator = new Frobnicator();
            frobnicator.do_something_else();
            """;

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("Failed to locate symbol 'do_something_else' in type IFrobnicator");
    }

    [Fact]
    public void implementing_method_without_implement_modifier_should_emit_expected_error()
    {
        string source = """
            public interface IFrobnicator
            {
                public frobnicate(): void;
            }

            public class Frobnicator : IFrobnicator
            {
                // This is expected to emit an error - missing 'implement' modifier.
                public frobnicate(): void
                {
                }
            }

            var frobnicator: IFrobnicator = new Frobnicator();
            frobnicator.frobnicate();
            """;

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("Method 'frobnicate' defined in interface 'IFrobnicator' is implemented in 'Frobnicator', but lacks the 'implement' method modifier.");
    }

    [Fact]
    public void implementing_method_not_present_in_interface_should_emit_expected_error()
    {
        string source = """
            public interface IFoo
            {
                public foo(): void;
            }

            public class Foo : IFoo
            {
                public implement foo(): void
                {
                }

                // There is no 'bar' method in the interface, so this should emit an error
                public implement bar(): void
                {
                }
            }
            """;

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("Method 'bar' defined in class 'Foo' is marked with 'implement' modifier but not present in any of the interfaces the class implements'");
    }

    [Fact]
    public void not_implementing_method_present_in_interface_should_emit_expected_error()
    {
        string source = """
            public interface IFoo
            {
                public foo(): void;
            }

            public class Foo : IFoo
            {
                // Foo does not implement 'foo', and is not defined as an 'abstract' class (a concept we currently do
                // not support, https://gitlab.perlang.org/perlang/perlang/-/work_items/66)
            }
            """;

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("Method 'foo' defined in interface 'IFoo' is not implemented in 'Foo'.");
    }

    [Fact]
    public void using_implement_modifier_in_interface_should_emit_expected_error()
    {
        string source = """
            public interface IFrobnicator
            {
                // The 'implement' modifier is not supported in this context
                public implement frobnicate(): void;
            }
            """;

        var result = EvalWithParseErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("'implement' modifier cannot be used in interfaces");
    }

    [Fact]
    public void using_implement_modifier_for_non_interface_method_should_emit_expected_error()
    {
        string source = """
            public class Foo
            {
                // There is no 'bar' method defined in any interface/base class for this class, so this should emit an
                // error
                public implement bar(): void
                {
                }
            }
            """;

        var result = EvalWithValidationErrorCatch(source);

        result.Errors.Should()
            .ContainSingle()
            .Which
            .Message.Should().Contain("Method 'bar' defined in class 'Foo' is marked with 'implement' modifier but not present in any of the interfaces the class implements");
    }
}
