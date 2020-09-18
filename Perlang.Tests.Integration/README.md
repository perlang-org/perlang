# Perlang.Tests.Integration

This project contains the integration tests for Perlang. The aim is to have all language features tests at an
appropriate level of coverage; line-based coverage is not enough in this case (preferably, each line should be
covered multiple times in most cases), but all keywords, expressions and statements should be properly tested.

The integration tests fire up a full Perlang interpreter for each test method. The [EvalHelper](EvalHelper.cs)
class provides useful helper methods for various ways to do this: run some code and make assertions based on the
value returned from the (single, but potentially nested) expression in the program, based on the output from the
program, or based on the types of errors that can occur during the various parts of the processing:

- `ScanError`: errors when scanning the source code and producing a stream of lexical tokens.
- `ParseError`: errors when constructing an AST (abstract syntax tree) for these tokens.
- `ResolveError`: errors when resolving (local and global) names of variables and functions.
- `TypeValidationError`: errors when performing static type analysis of the program.
- `RuntimeError`: runtime errors.

For certain scenarios, it might make more sense to implement the tests as unit tests instead of integration tests.
See the [Perlang.Tests](../Perlang.Tests) project for the location where these tests can be placed.

`-- Per Lundberg <perlun@gmail.com>  2020-09-16 22:26:42 +0300`
