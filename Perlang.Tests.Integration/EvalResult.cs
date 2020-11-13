#nullable enable
using System;
using System.Collections.Generic;

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
        /// Gets a list of the errors thrown during the evaluation of the program.
        /// </summary>
        public List<T> Errors { get; } = new List<T>();
    }
}
