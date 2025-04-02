#nullable enable
#pragma warning disable SA1010

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using Perlang.Interpreter.NameResolution;

namespace Perlang.Interpreter.Immutability
{
    /// <summary>
    /// Validator which ensures that objects which are intended to be immutable (either by nature or because they have
    /// been configured in a particular way) cannot be mutated.
    /// </summary>
    internal class ImmutabilityValidator : VisitorBase
    {
        private readonly Action<ValidationError> immutabilityValidationErrorCallback;
        private readonly Func<Expr, Binding?> getVariableOrFunctionBinding;
        private ConcurrentDictionary<Stmt.Class, Dictionary<Stmt.Field, bool>> FieldAssignedTo { get; } = new();
        private bool isCurrentlyInConstructor;

        public static void Validate(
            ImmutableList<Stmt> statements,
            Action<ValidationError> immutabilityValidationErrorCallback,
            Func<Expr, Binding?> getVariableOrFunctionBinding)
        {
            new ImmutabilityValidator(immutabilityValidationErrorCallback, getVariableOrFunctionBinding).Visit(statements);
        }

        private ImmutabilityValidator(
            Action<ValidationError> immutabilityValidationErrorCallback,
            Func<Expr, Binding?> getVariableOrFunctionBinding)
        {
            this.immutabilityValidationErrorCallback = immutabilityValidationErrorCallback;
            this.getVariableOrFunctionBinding = getVariableOrFunctionBinding;
        }

        public override VoidObject VisitAssignExpr(Expr.Assign expr)
        {
            base.VisitAssignExpr(expr);

            Binding? binding = getVariableOrFunctionBinding(expr);

            if (binding is FieldBinding fieldBinding) {
                // Binding being 'null' here can either be because of an internal error (failure to locate a binding that
                // _should_ exist), or a completely valid case when trying to reassign an undefined variable. Regretfully,
                // we cannot distinguish between these two scenarios at the moment.
                if (binding.IsImmutable) {
                    if (fieldBinding.Field.Initializer != null) {
                        immutabilityValidationErrorCallback(new ImmutabilityValidationError(
                            expr.TargetName,
                            $"{binding.ObjectTypeTitleized} '{expr.TargetNameString}' cannot be assigned to; the field is immutable and has already been initialized."
                        ));
                    }
                    else if (FieldAssignedTo.GetOrAdd(fieldBinding.Class, []).GetValueOrDefault(fieldBinding.Field, false)) {
                        immutabilityValidationErrorCallback(new ImmutabilityValidationError(
                            expr.TargetName,
                            $"{binding.ObjectTypeTitleized} '{expr.TargetNameString}' cannot be assigned to; the field is immutable and has already been assigned to."
                        ));
                    }
                    else if (isCurrentlyInConstructor) {
                        // TODO: Make this check more strict by replacing isCurrentlyInConstructor with an
                        // TODO: "inConstructorForClass" field instead (which can contain an Stmt.Class value). The
                        // TODO: reason why I'm not doing it right now is because it also means we should add an extra
                        // TODO: "else" block for handling cases when we are not in the class' own constructor here.

                        // The "isCurrentlyInConstructor" check is important, so that the "all fields are initialized"
                        // check in VisitClassStmt() can succeed.
                        FieldAssignedTo.GetOrAdd(fieldBinding.Class, [])[fieldBinding.Field] = true;
                    }
                }
                else if (isCurrentlyInConstructor) {
                    // This branch handles mutable fields; it's important to populate the FieldInitialized structure for
                    // such fields too, so we can detect "all fields initialized" or not.
                    FieldAssignedTo.GetOrAdd(fieldBinding.Class, [])[fieldBinding.Field] = true;
                }
            }
            else if (binding?.IsImmutable == true) {
                immutabilityValidationErrorCallback(new ImmutabilityValidationError(
                    expr.TargetName,
                    $"{binding.ObjectTypeTitleized} '{expr.TargetNameString}' is immutable and cannot be modified."
                ));
            }

            return VoidObject.Void;
        }

        public override VoidObject VisitFunctionStmt(Stmt.Function stmt)
        {
            if (stmt.IsConstructor) {
                isCurrentlyInConstructor = true;

                // We clear this everytime we enter a constructor, since each constructor must make sure to initialize
                // all fields.
                FieldAssignedTo.GetOrAdd(stmt.Class, []).Clear();
            }

            base.VisitFunctionStmt(stmt);

            if (stmt.IsConstructor) {
                // Ensure all fields are properly initialized now. It's important to do it at this specific point, since
                // there might be multiple constructors defined and each and every constructor must be checked.
                var fieldInitialized = FieldAssignedTo[stmt.Class];

                foreach (Stmt.Field field in stmt.Class.Fields) {
                    if (field.Initializer != null) {
                        continue;
                    }

                    // This check technically has nothing to do with the immutability validation, but it makes sense to
                    // keep it here since it relies on state collected during the immutability validation phase.
                    if (!fieldInitialized.GetValueOrDefault(field, false)) {
                        immutabilityValidationErrorCallback(new ImmutabilityValidationError(
                            field.Name,
                            $"Field '{field.Name.Lexeme}' in class '{stmt.Class.Name}' was not initialized in field initializer or constructor."
                        ));
                    }
                }

                isCurrentlyInConstructor = false;
            }

            return VoidObject.Void;
        }

        public override VoidObject VisitClassStmt(Stmt.Class stmt)
        {
            base.VisitClassStmt(stmt);

            foreach (Stmt.Function method in stmt.Methods) {
                if (method.IsConstructor) {
                    // No need to continue - the uninitialized fields detection has already been handled elsewhere.
                    return VoidObject.Void;
                }
            }

            var fieldInitialized = FieldAssignedTo.GetOrAdd(stmt, []);

            foreach (Stmt.Field field in stmt.Fields) {
                if (field.Initializer != null) {
                    continue;
                }

                // This check technically has nothing to do with the immutability validation, but it makes sense to
                // keep it here since it relies on state collected during the immutability validation phase.
                if (!fieldInitialized.GetValueOrDefault(field, false)) {
                    immutabilityValidationErrorCallback(new ImmutabilityValidationError(
                        field.Name,
                        $"Field '{field.Name.Lexeme}' in class '{stmt.Name}' was not initialized in field initializer, and no constructors have been defined."
                    ));
                }
            }

            return VoidObject.Void;
        }
    }
}
