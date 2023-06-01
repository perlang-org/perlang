using System;
using System.Collections.Generic;
using Perlang.Interpreter.NameResolution;
using Perlang.Parser;

namespace Perlang.Interpreter.Typing
{
    /// <summary>
    /// Tree-walker which validates all explicit and implicit type references in an expression tree.
    ///
    /// Each expression is populated with the "aggregate type" of its descendants, recursively. The "aggregate type"
    /// is determined by walking the tree and concluding what the final type to evaluate the expression will be. For
    /// example, adding two integers (1 + 1) will have an aggregate type of `int`. Adding
    /// an integer and a float (1 + 1.0) will have an aggregate type of `float`, and so forth.
    ///
    /// Adding a smaller type and a larger type (e.g. `int` and `float`) will expand the expression to the greater of
    /// the involved types. Note that this can lose some precision when expanding `double` or `float` values to the CLR
    /// `decimal` type, but the order of magnitude is always retained.
    ///
    /// For expression trees where no clear aggregate type can be determined, or where an aggregate type would be
    /// potentially confusing to the user, a result indicating this is returned to the caller. We try to follow the
    /// good old Python rule of "explicit is better than implicit", where relevant.
    /// </summary>
    internal static class TypeValidator
    {
        public static void Validate(
            IList<Stmt> statements,
            Action<TypeValidationError> typeValidationErrorCallback,
            Func<Expr, Binding> getVariableOrFunctionCallback,
            Action<CompilerWarning> compilerWarningCallback)
        {
            bool typeResolvingFailed = false;

            //
            // Phase 1: Resolve explicit and implicit type references to their corresponding CLR types.
            //
            var typeResolver = new TypeResolver(
                getVariableOrFunctionCallback,
                validationError =>
                {
                    typeValidationErrorCallback(validationError);
                    typeResolvingFailed = true;
                }
            );

            try
            {
                typeResolver.Resolve(statements);
            }
            catch (TypeValidationError e)
            {
                // Some errors are handled gracefully by the Validate() method, while others cause an exception to
                // be thrown and the rest of the validation to abort. We handle both kinds and invoke the callback
                // in either cause, doing our very best to ensure exceptions are not propagated to the caller.
                typeValidationErrorCallback(e);
                return;
            }

            if (typeResolvingFailed)
            {
                // Something went wrong already at the type resolving stage. Possible causes for this could be
                // references to undefined variables. All errors have been reported to the caller at this point.
                // Since the resolving failed, we must not continue the processing since the expression tree(s) can
                // not be guaranteed to be in a healthy state at this stage. It is quite likely that subsequent
                // exceptions would be caused because of errors which are already reported upstream.
                return;
            }

            //
            // Phase 2: Validate that type resolving worked.
            //

            // The whole expression tree should be walked by now; any type references still not resolved at this point
            // is a critical error that should fail the type validation. To provide as much information to the user
            // as possible, the full list errors (if any) are reported back to the caller; we don't just stop at the
            // first error encountered. (The compiler could potentially discard information except for the first n
            // errors if desired, though. The key point here is to not discard it at the wrong stage in the pipeline.)
            new TypesResolvedValidator(getVariableOrFunctionCallback, typeValidationErrorCallback, compilerWarningCallback)
                .ReportErrors(statements);

            //
            // Phase 3: Ensure that no assignments are made from incoercible values
            //

            // An example error that the above detects is the following:
            //
            // var i = 1;
            // i = "foo"; // error
            //
            // Once a variable has been defined, it's type has been set; it cannot be reassigned with a value of a
            // completely different type. The only exception to this rule is when a smaller numeric value (e.g. `int`)
            // is expanded to a larger type (e.g. `long`).
            new TypeAssignmentValidator(getVariableOrFunctionCallback, typeValidationErrorCallback)
                .ReportErrors(statements);

            //
            // Phase 4: Ensure that all expressions involving boolean operands have operands of `bool`.
            //

            // These expressions are things like `foo && bar`, `if (baz) { ... }` and `while (zot) { ... }`. All of
            // these require proper, boolean operands and should trigger a compile-time error if used with any other
            // types of operands.
            new BooleanOperandsValidator(getVariableOrFunctionCallback, typeValidationErrorCallback)
                .ReportErrors(statements);
        }
    }
}
