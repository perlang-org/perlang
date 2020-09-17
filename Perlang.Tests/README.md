# Perlang.Tests

The `Perlang.Tests` project contains tests which are more "unit-tests" in nature. This is defined as _tests that do not
require a full Perlang interpreter to run_. For a programming language interpreter/compiler, integration tests often
provide more value than pure unit tests (and allow us to test all relevant code paths

However, there are still aspects of the system where it makes sense and simplifies the development to be able to test
individual methods as they are being developed. This is where the `Perlang.Tests` project come into picture.. Unit tests 
also have another value in and of itself: it forces you to write decoupled, testable code. From my experience, the only
way to ensure that you have code that is really testable is by _writing tests for it_.  

For the integration tests, see the [Perlang.Tests.Integration](../Perlang.Tests.Integration) project.

-- Per Lundberg <perlun@gmail.com>  2020-09-16 22:22:02 +0300
