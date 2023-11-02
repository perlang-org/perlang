#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Perlang.Parser;

namespace Perlang.Tests.Integration
{
    /// <summary>
    /// Represents a result from a single `Eval` invocation, with support for returning a single type of errors as
    /// specified by the `T` type parameter.
    /// </summary>
    /// <typeparam name="T">The type of the error.</typeparam>
    internal class EvalResult<T>
        where T : Exception
    {
        /// <summary>
        /// Gets or sets the value returned from the `Eval` call. Can be `null` if the source code evaluated didn't
        /// parse cleanly as a valid expression, or if it parsed as a set of statements.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Gets a collection of all lines printed to the output stream while executing the program.
        /// </summary>
        public IReadOnlyList<string> Output => output.AsReadOnly();

        private readonly List<string> output = new();

        public IEnumerable<char> OutputAsString => String.Join("\n", output);

        /// <summary>
        /// Gets a collection of the errors thrown during the evaluation of the program.
        /// </summary>
        public ReadOnlyCollection<T> Errors => errors.AsReadOnly();

        private readonly List<T> errors = new();

        /// <summary>
        /// Gets a collection of the compiler warnings emitted during the compilation of the program.
        /// </summary>
        public IReadOnlyList<CompilerWarning> CompilerWarnings => compilerWarnings.AsReadOnly();

        /// <summary>
        /// Gets or sets the path to the executable which was created as part of this evaluation. Will be `null` when
        /// running in interpreted mode.
        /// </summary>
        public string? ExecutablePath { get; set; }

        private readonly List<CompilerWarning> compilerWarnings = new();

        public void ErrorHandler(T error)
        {
            errors.Add(error);
        }

        public void OutputHandler(Lang.String line)
        {
            // TODO: Store as native Perlang strings when we are ready for it. The problem right now is that it makes
            // TODO: the unit/integration test code at lot more verbose (since we can't use standard string constants)
            // TODO: for too little benefit.
            output.Add(line.ToString());
        }

        public bool WarningHandler(CompilerWarning compilerWarning)
        {
            compilerWarnings.Add(compilerWarning);

            // Indicate to the caller that this warning is not considered an error.
            return false;
        }
    }
}
